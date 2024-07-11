using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Amazon.Runtime;
using System.Linq;

public class InputProcessor : MonoBehaviour
{
    LevelController levelController = LevelController.Instance;

    public TMP_InputField textMeshProInputField;

    private Dictionary<string, Action<string>> entityFunctions;
    private Dictionary<string, Entity> entities;
    public Board board;
    public float stepDelay = 1f; // Time delay between each step, adjustable in the inspector
    private GameController gameController;

    private string input = "";

    [SerializeField] private Dictionary<string, object> variableValues = new Dictionary<string, object>();

    [SerializeField] private int maxWhileLoop = 10;
    [SerializeField] private int maxLoopDepth = 10;
    private bool stopRequested = false; // Flag to stop execution

    [SerializeField] private bool breakWhile = false;
    [SerializeField] private bool continueInsideIf = false;
    private bool insideIf = false;

    private string previousAttempt = "I have not attempted anything";

    public LogsToTextComponent logsToTextComponent;

    void Start()
    {
        gameController = GetComponent<GameController>();
        if (gameController == null)
        {
            return;
        }
        string map = gameController.GetMap();
        gameController.Initialise(map, out board, out entities, out entityFunctions);

        if (!textMeshProInputField)
        {
            Debug.LogError("Script Editor not attached");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopExecution();
        }
    }

    IEnumerator ExecuteSequentially(string input)
    {
        // Wait for ResetPositions to complete
        yield return StartCoroutine(Reset());

        // Now run ProcessInput
        yield return StartCoroutine(ProcessInput(input));

        //This is the end of the attempt
        //Did we win? - if not, then end the attempt, otherwise it is sorted out by the other components
        if (!gameController.winScreenShown())
        {
            gameController.failedAttempt();
        }
        else
        {
            gameController.WinAttempt();
        }
    }

    public void StartExecution()
    {
        input = textMeshProInputField.text;
        previousAttempt = input;
        stopRequested = false; // Reset the stop flag
        board.startPlay();
        StartCoroutine(ExecuteSequentially(input));
        
    }

    public string GetCode()
    {
        return input;
    }

    public string getPreviousAttempt()
    {
        return previousAttempt;
    }

    public void StopExecution()
    {
        stopRequested = true; // Set the stop flag
    }


    public void Restart()
    {
        gameController.RestartMade();
        StartCoroutine(Reset());
    }
    

    private IEnumerator Reset()
    {
        board.Reset();
        logsToTextComponent.Reset();
        //Reset the variables being used
        variableValues = new Dictionary<string, object>();
        yield return new WaitForSeconds(stepDelay);
    }

    void ListKeyValuePairs()
    {
        Debug.Log("Below are all the key value pairs for current scope...");
        foreach (var kvp in variableValues)
        {
            Debug.Log($"\tKey: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    private IEnumerator ProcessInput(string input)
    {
        if (stopRequested)
        {
            yield break; // Stop if requested
        }

        if (!HasEvenBrackets(input))
        {
            Debug.LogError("You have a mismatched bracket");
            yield return null;
        }

        var lines = input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        variableValues = new Dictionary<string, object>(); // Initialize the scope
        yield return StartCoroutine(ExecuteLines(lines, 0));
    }

    private IEnumerator ExecuteLines(string[] lines, int depth)
    {
        List<string> currentBlock = new List<string>();
        int lineNumber = 0;
        while (lineNumber < lines.Length)
        {
            if (stopRequested)
            {
                yield break; // Stop if requested
            }

            //Propagates from the nested if statement
            if(continueInsideIf)
            {
                continueInsideIf = false;
                yield break;
            }

            var trimmedLine = lines[lineNumber].Trim();
            //Now we check if there was a break needed to be made.
            if (isBreak(trimmedLine))
            {
                //This needs to cause the while loop to stop.
                breakWhile = true;
                yield break;
            }
            else if (isContinue(trimmedLine))
            {
                //This exits this loop of lines to be executed but it does not cause the while loop to be broken.
                if (insideIf)
                {
                    continueInsideIf = true;
                    insideIf = false;
                }
                yield break;
            }


            if (IsControlFlowStart(trimmedLine, out string controlType, out string controlExpression))
            {
                currentBlock = new List<string>();
                int recursionCheck = 0;
                if (trimmedLine.Contains("{"))
                {
                    recursionCheck++;
                }
                lineNumber++;
                while (lineNumber < lines.Length)
                {
                    trimmedLine = lines[lineNumber].Trim();
                    if (trimmedLine.Contains('}') || trimmedLine.Contains('{'))
                    {
                        if (trimmedLine.Contains("}"))
                        {
                            if (recursionCheck > 1)
                            {
                                recursionCheck--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (trimmedLine.Contains("{"))
                        {
                            recursionCheck++;
                        }
                        
                    }                    
                    else
                    {
                        currentBlock.Add(trimmedLine);
                    }
                    lineNumber++;
                }
                //Check for an else statement if an if statement was used
                if (controlType == "if" && trimmedLine.Contains("else"))
                {
                    List<string> elseBlock = new List<string>();
                    recursionCheck = 0;
                    if (trimmedLine.Contains("{"))
                    {
                        recursionCheck++;
                    }
                    lineNumber++;
                    while (lineNumber < lines.Length)
                    {
                        trimmedLine = lines[lineNumber].Trim();
                        if (trimmedLine == "{")
                        {
                            recursionCheck++;
                        }
                        else if (trimmedLine == "}")
                        {
                            if (recursionCheck > 1)
                            {
                                recursionCheck--;
                            }
                            else
                            {
                                break;
                            }
                        } else
                        {
                            elseBlock.Add(trimmedLine);
                        }
                        lineNumber++;
                    }
                    yield return StartCoroutine(HandleIfElseStatement(controlExpression, currentBlock.ToArray(), elseBlock.ToArray(), depth + 1));
                } else
                {
                    yield return StartCoroutine(HandleControlFlow(controlType, controlExpression, currentBlock.ToArray(), depth + 1));
                }
            }
            else if (IsVariableAssignment(trimmedLine, out string varName, out string varValue, out string varType))
            {
                if (variableValues.ContainsKey(varName))
                {
                    ListKeyValuePairs();
                    Debug.LogError($"Variable '{varName}' already exists in the current scope.");
                    yield break;
                }

                object evaluatedValue = EvaluateValue(varType, varValue);
                variableValues[varName] = evaluatedValue;
            }
            else if (isVariableReAssignment(trimmedLine, out varName, out varValue, out varType))
            {
                if (!doesVariableExists(varName))
                {
                    Debug.LogError($"Variable: '{varName}' does not exist in the current scope");
                }

                string inferredType = InferVariableType(varValue);

                object evaluatedValue = EvaluateValue(inferredType, varValue);

                if (variableValues[varName].GetType() != evaluatedValue.GetType())
                {
                    Debug.LogError($"Type mismatch: Cannot assign {evaluatedValue.GetType()} to {variableValues[varName].GetType()}");
                    yield break;
                }
                variableValues[varName] = evaluatedValue;
            } else if (isCallingSignFunction(trimmedLine))
            {
                Debug.LogError($"Nothing to assign the response to for line `{trimmedLine}`");
                
            } else
            {
                yield return StartCoroutine(ExecuteLine(trimmedLine));
            }
            lineNumber++;
        }
    }
    
    private bool isCallingSignFunction(string trimmedLine)
    {
        if (trimmedLine.StartsWith("sign."))
        {
            return true;
        }
        return false;
    }

    private bool isBreak(string trimmedLine)
    {
        if (trimmedLine == "break")
        {
            return true;
        } else
        {
            return false;
        }
    }

    private bool isContinue(string trimmedLine)
    {
        if (trimmedLine == "continue")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool doesVariableExists(string varName)
    {
        if (variableValues.ContainsKey(varName))
        {
            return true;
        }
        return false;
    }

    private bool IsControlFlowStart(string line, out string controlType, out string controlExpression)
    {
        controlType = null;
        controlExpression = null;
        //Check to see if it should be a while or an if
        if (line.StartsWith("while") || line.StartsWith("if") || line.StartsWith("for"))
        {
            if (line.StartsWith("while("))
            {
                controlType = "while";
                controlExpression = ExtractExpression(line);
                return true;
            }
            else if (line.StartsWith("if("))
            {
                controlType = "if";
                controlExpression = ExtractExpression(line);
                return true;
            }
            else if (line.StartsWith("for("))
            {
                controlType = "for";

                controlExpression = ExtractExpression(line);
                return true;
            }
            else
            {
                Debug.LogError($"Incorrect declaration of loop: {line}");
            }
        }
        

        return false;
    }

    private IEnumerator ExecuteLine(string line)
    {
        var parts = line.Split(new[] { '.' }, 2);
        if (parts.Length < 2) yield break;

        var entityName = parts[0];
        var commandPart = parts[1].Split(new[] { '(', ')' });

        var commandName = commandPart[0];
        var param = commandPart.Length > 1 ? commandPart[1] : "";

        if (variableValues.ContainsKey(param))
        {
            param = variableValues[param].ToString();
        }

        if (entities.ContainsKey(entityName) && entityFunctions.ContainsKey(commandName))
        {
            Debug.Log($"Running Command: {commandName} with parameter: {param}");
            entityFunctions[commandName].Invoke(param);
            board.UpdateBoard();
            yield return new WaitForSeconds(stepDelay);
        }
        else
        {
            Debug.LogError($"Invalid command: {line}");
        }
    }

    private IEnumerator HandleControlFlow(string controlType, string expression, string[] steps, int depth)
    {
        if (depth > maxLoopDepth)
        {
            Debug.LogError("TOO many recursive loops");
            yield break;
        }

        int loopCount = 0;
        switch (controlType)
        {
            case "while":
                loopCount = 0;
                Debug.Log($"Evaluating this expression: {expression}");
                while (EvaluateExpression(expression))
                {
                    if (loopCount >= maxLoopDepth)
                    {
                        Debug.LogError($"Infinite Loop Detected, while loop exceded {maxLoopDepth} times");
                        break;
                    }
                    if (stopRequested || breakWhile)
                    {
                        yield break; // Stop if requested
                    }
                    loopCount++;
                    yield return StartCoroutine(ExecuteLines(steps, depth));
                    yield return new WaitForSeconds(stepDelay);
                }
                Debug.Log($"Finished with the while loop: {expression}");
                break;

            case "if":
                if (EvaluateExpression(expression))
                {
                    insideIf = true;
                    yield return StartCoroutine(ExecuteLines(steps, depth));
                    yield return new WaitForSeconds(stepDelay);
                }
                break;
            case "for":
                loopCount = 0;
                int range = EvaluateRange(ExtractForLoopExpression(expression));
                string counterString = EvaluateCounter(expression);
                int counter = int.Parse(variableValues[counterString].ToString());
                while (counter < range) {
                    if (loopCount >= maxLoopDepth)
                    {
                        Debug.LogError($"Infinite Loop Detected, while loop exceded {maxLoopDepth} times");
                        break;
                    }
                    if (stopRequested || breakWhile)
                    {
                        yield break; // Stop if requested
                    }
                    variableValues[counterString] = int.Parse(variableValues[counterString].ToString()) + 1;
                    counter = int.Parse(variableValues[counterString].ToString());
                    loopCount++;
                    yield return StartCoroutine(ExecuteLines(steps, depth));
                    yield return new WaitForSeconds(stepDelay);
                }
                break;

            default:
                Debug.LogError($"Unsupported control type: {controlType}");
                break;
        }
    }

    private IEnumerator HandleIfElseStatement(string expression, string[] ifSteps, string[] elseSteps, int depth)
    {
        if (depth > maxLoopDepth)
        {
            Debug.LogError("TOO many recursive loops");
            yield break;
        }

        if (EvaluateExpression(expression))
        {
            insideIf = true;
            yield return StartCoroutine(ExecuteLines(ifSteps, depth));
            yield return new WaitForSeconds(stepDelay);
        } else
        {
            insideIf = true;
            yield return StartCoroutine(ExecuteLines(elseSteps, depth));
            yield return new WaitForSeconds(stepDelay);
        }
    }


    private int EvaluateRange(string expression)
    {
        //should either be a number of a variable or something that becomes a number eventually
        if (int.TryParse(expression, out var range))
        {
            return range;
        }
        //It needs to be a variable in this case
        if (variableValues.ContainsKey(expression) && int.TryParse(variableValues[expression].ToString(), out var dicVal))
        {
            return int.Parse(variableValues[expression].ToString());
        }
        throw new FormatException($"Invalid range format: {expression}");
    }

    private string EvaluateCounter(string expression)
    {
        //In the form of 'i in range(10)'
        // Find the starting index of 'range('
        int startIndex = 0;

        // Find the closing parenthesis for 'range(...)'
        int endIndex = expression.IndexOf(' ', startIndex);
        if (endIndex == -1)
        {
            throw new InvalidOperationException($"Incorrect format of for loop: {expression}, no closing paranthesis");
        }

        // Extract the content inside the parentheses
        string line = expression.Substring(startIndex, endIndex).Trim();

        if (variableValues.ContainsKey(line))
        {
            throw new InvalidOperationException($"The variable {line} already exists or ");
        }
        variableValues[line] = 0;
        return line;

    }

    private bool EvaluateExpression(string expression)
    {
        try
        {
            string entityName, methodName;
            bool negate = expression.StartsWith("!");

            if (negate)
            {
                expression = expression.Substring(1);
            }

            if (expression.Contains(">") || expression.Contains("<") || expression.Contains("==") || expression.Contains("!="))
            {
                return EvaluateRelationalExpression(expression);
            } else if (expression.Contains("and") || expression.Contains("or"))
            {
                return EvaluateBooleanExpression(expression);
            }



            var parts = expression.Split(new[] { '.' }, 2);
            if (parts.Length < 2)
            {
                if (variableValues.ContainsKey(expression))
                {
                    bool variableResult = (bool)variableValues[expression];
                    return negate ? !variableResult : variableResult;
                }

                throw new FormatException($"Invalid expression format: {expression}");
            }

            entityName = parts[0];
            methodName = parts[1].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (!entities.ContainsKey(entityName))
            {
                throw new KeyNotFoundException($"Entity '{entityName}' not found.");
            }

            var entity = entities[entityName];
            var method = entity.GetType().GetMethod(methodName);
            if (method == null)
            {
                throw new MissingMethodException($"Method '{methodName}' not found in entity '{entityName}'.");
            }

            Debug.Log($"Calling method: {methodName} on entity: {entityName}");

            bool result = (bool)method.Invoke(entity, null);
            return negate ? !result : result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error evaluating expression '{expression}': {ex.Message}");
            return false;
        }
    }

    private bool EvaluateBooleanExpression(string expression)
    {
        string[] operators = new[] { "and", "or"};
        foreach (var op in operators)
        {
            if (expression.Contains(op))
            {
                var parts = expression.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid boolean expression format: {expression}");
                }

                var left = parts[0].Trim();
                var right = parts[1].Trim();
                object leftValue = EvaluateExpressionPart(left);
                object rightValue = EvaluateExpressionPart(right);
                return EvaluateOperation(leftValue, rightValue, op);
            }
        }
        throw new FormatException($"Unsupported boolean operator in expression: {expression}");


    }

    private bool EvaluateRelationalExpression(string expression)
    {
        string[] operators = new[] { ">", "<", "==", "!=" };
        foreach (var op in operators)
        {
            if (expression.Contains(op))
            {
                var parts = expression.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid relational expression format: {expression}");
                }

                var left = parts[0].Trim();
                var right = parts[1].Trim();
                object leftValue = EvaluateExpressionPart(left);
                object rightValue = EvaluateExpressionPart(right);
                return EvaluateOperation(leftValue, rightValue, op);
            }
        }
        throw new FormatException($"Unsupported relational operator in expression: {expression}");


    }

    private object EvaluateExpressionPart(string part)
    {
        //Check if the expression part is multiple parts
        var additionalParts = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (additionalParts.Length == 3)
        {
            var left = additionalParts[0].Trim();
            var op = additionalParts[1].Trim();
            var right = additionalParts[2].Trim();

            int leftValue;
            int rightValue;

            // Evaluate left side
            if (variableValues.ContainsKey(left))
            {
                leftValue = Convert.ToInt32(variableValues[left]);
            }
            else if (int.TryParse(left, out int parsedLeft))
            {
                leftValue = parsedLeft;
            }
            else
            {
                throw new FormatException($"Invalid left value '{left}' in expression.");
            }

            // Evaluate right side
            if (variableValues.ContainsKey(right))
            {
                rightValue = Convert.ToInt32(variableValues[right]);
            }
            else if (int.TryParse(right, out int parsedRight))
            {
                rightValue = parsedRight;
            }
            else
            {
                throw new FormatException($"Invalid right value '{right}' in expression.");
            }

            // Perform the operation
            switch (op)
            {
                case "-":
                    return leftValue - rightValue;
                case "+":
                    return leftValue + rightValue;
                case "*":
                    return leftValue * rightValue;
                case "/":
                    return leftValue / rightValue;
                case "%":
                    return leftValue % rightValue;
                default:
                    throw new FormatException($"'{op}' is not a supported calculation.");
            }
        }
        else if (additionalParts.Length != 1)
        {
            throw new FormatException($"'{part}' is not a valid expression.");
        }


        if (variableValues.ContainsKey(part))
        {
            return variableValues[part];
        }

        if (int.TryParse(part, out int intValue))
        {
            return intValue;
        }
        if (bool.TryParse(part, out bool boolValue))
        {
            return boolValue;
        }
        if (part.StartsWith("\"") && part.EndsWith("\""))
        {
            return part.Substring(1, part.Length - 2);
        }
        // Add more types if needed
        Debug.LogError($"No value able to be associated with {part}");
        return part;
    }

    private bool EvaluateOperation(object leftValue, object rightValue, string op)
    {
        if ((op == ">" || op == "<") && (!int.TryParse(leftValue.ToString(), out int leftInt) || !int.TryParse(rightValue.ToString(), out int rightInt)))
        {
            throw new ArgumentException("Both leftValue and rightValue must be convertible to integers for > and < operators.");
        }

        if ((op == "or" || op == "and") && (!bool.TryParse(leftValue.ToString(), out bool leftBool) || !bool.TryParse(rightValue.ToString(), out bool rightBool)))
        {
            throw new ArgumentException("Both leftValue and rightValue must be convertible to integers for 'and' and 'or' operators.");
        }

        switch (op)
        {
            case ">":
                return int.Parse(leftValue.ToString()) > int.Parse(rightValue.ToString());
            case "<":
                return int.Parse(leftValue.ToString()) < int.Parse(rightValue.ToString());
            case "==":
                return leftValue.Equals(rightValue);
            case "!=":
                return !leftValue.Equals(rightValue);
            case "and":
                return bool.Parse(leftValue.ToString()) && bool.Parse(rightValue.ToString());
            case "or":
                return bool.Parse(leftValue.ToString()) || bool.Parse(rightValue.ToString());
            default:
                throw new InvalidOperationException($"Unsupported operator: {op}");
        }
    }

    private string ExtractExpression(string line)
    {
        int startIndex = line.IndexOf('(') + 1;
        int endIndex = line.LastIndexOf(')');
        return line.Substring(startIndex, endIndex - startIndex);
    }

    private string ExtractForLoopExpression(string line)
    {
        //In the form of 'for(i in range(10))'
        // Find the starting index of 'range('
        int rangeStartIndex = line.IndexOf("range(");
        if (rangeStartIndex == -1)
        {
            throw new InvalidOperationException($"Incorrect format of for loop: {line}, range not implemented correctly");
        }

        // Calculate the index where the content inside the parentheses starts
        int expressionStartIndex = rangeStartIndex + "range(".Length;

        // Find the closing parenthesis for 'range(...)'
        int rangeEndIndex = line.IndexOf(')', expressionStartIndex);
        if (rangeEndIndex == -1)
        {
            throw new InvalidOperationException($"Incorrect format of for loop: {line}, no closing paranthesis");
        }

        // Extract the content inside the parentheses
        string expression = line.Substring(expressionStartIndex, rangeEndIndex - expressionStartIndex).Trim();

        return expression;
    }

    private bool IsVariableDeclaration(string line, out string varType, out string varName, out string varValue)
    {
        varType = null;
        varName = null;
        varValue = null;

        var parts = line.Split(new[] { ' ' }, 3);
        if (parts.Length == 3 && (parts[0] == "int" || parts[0] == "bool" || parts[0] == "string"))
        {
            varType = parts[0];
            varName = parts[1];
            varValue = parts[2];
            return true;
        }

        return false;
    }

    private bool IsVariableAssignment(string line, out string varName, out string varValue, out string varType)
    {

        varName = null;
        varValue = null;
        varType = null;

        // Check for variable declaration and assignment (e.g., int distance = 1)
        var declarationParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (declarationParts.Length >= 4 && declarationParts[2].Contains('='))
        {
            varType = declarationParts[0].Trim();
            varName = declarationParts[1].Trim();
            // e.g. int distance = 1
            // Get the value from second part when split by the '='
            var assignmentParts = line.Split(new [] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (assignmentParts.Length == 2)
            {
                varValue = assignmentParts[1].Trim();
                return true;
            }
            Debug.LogError($"Incorrect assignment of a variable {line}");
        }
        return false;
    }

    private bool isVariableReAssignment(string line, out string varName, out string varValue, out string varType)
    {
        varName = null;
        varValue = null;
        varType = null;
        // Check for variable reassignment (e.g., distance = distance - 1)
        var assignmentParts = line.Split(new[] { ' '}, 3);
        if (assignmentParts.Length > 2 && assignmentParts[1].Contains('=')) {
            var parts = line.Split(new[] { '='}, 2);
            varName = parts[0].Trim();
            varValue = parts[1].Trim();
            return true;
        }
        return false;
    }

    private string EvaluateSignFunction(string functionCall)
    {
        string function = functionCall.Substring("sign.".Length);
        if (function == "readSign()")
        {
            string signResult = board.GetSign();
            if(signResult == null)
            {
                Debug.LogError("There is no sign at this location");
            }
            return signResult;
        }
        Debug.Log($"the function `{function}` was not a known function of sign");
        return null;
    }

    private object EvaluateValue(string varType, string value)
    {
        if (isCallingSignFunction(value))
        {
            return EvaluateSignFunction(value);
        }
        // Check if the value is an expression like distance - 1
        var additionalParts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (additionalParts.Length == 3)
        {
            var left = additionalParts[0].Trim();
            var op = additionalParts[1].Trim();
            var right = additionalParts[2].Trim();

            int leftValue;
            int rightValue;

            // Evaluate left side
            if (variableValues.ContainsKey(left))
            {
                leftValue = Convert.ToInt32(variableValues[left]);
            }
            else if (int.TryParse(left, out int parsedLeft))
            {
                leftValue = parsedLeft;
            }
            else
            {
                throw new FormatException($"Invalid left value '{left}' in expression.");
            }

            // Evaluate right side
            if (variableValues.ContainsKey(right))
            {
                rightValue = Convert.ToInt32(variableValues[right]);
            }
            else if (int.TryParse(right, out int parsedRight))
            {
                rightValue = parsedRight;
            }
            else
            {
                throw new FormatException($"Invalid right value '{right}' in expression.");
            }

            // Perform the operation
            switch (op)
            {
                case "-":
                    return Convert.ToInt32(leftValue - rightValue);
                case "+":
                    return Convert.ToInt32(leftValue + rightValue);
                case "*":
                    return Convert.ToInt32(leftValue * rightValue);
                case "/":
                    return Convert.ToInt32(leftValue / rightValue);
                default:
                    throw new FormatException($"'{op}' is not a supported calculation.");
            }
        }
        else if (additionalParts.Length != 1)
        {
            throw new FormatException($"'{value}' is not a valid expression.");
        }

        // Check if the value is a known variable
        if (variableValues.ContainsKey(value))
        {
            return variableValues[value];
        }
        // Convert the value to the appropriate type
        switch (varType)
        {
            case "Int32":
            case "int":
                return int.Parse(value);
            case "Boolean":
                return bool.Parse(value);
            case "Single":
                return float.Parse(value);
            case "String":
            default:
                return value;
        }
    }
    
    private string InferVariableType(string value)
    {
        if (variableValues.ContainsKey(value))
        {
            return variableValues[value].GetType().Name;
        }

        if (int.TryParse(value, out _))
        {
            return "Int32";
        }
        if (bool.TryParse(value, out _))
        {
            return "Boolean";
        }
        if (float.TryParse(value, out _))
        {
            return "Single";
        }

        string[] op = { "-", "+", "*", "/", "%" };
        // Check to see if it is using a name of a variable
        string[] parts = value.Split(' ');
        List<String> values = new List<String>();
        for (int i = 0; i < parts.Length; i++)
        {
            if (!op.Contains(parts[i])) {
                values.Add(InferVariableType(parts[i]));
            }
        }
        bool allEqual = true;
        for (int i = 0; i < values.Count - 1; i++)
        {
            if (values[i] != values[i + 1])
            {
                Debug.Log($"They are not equal {values[i]} {values[i + 1]}");
                allEqual = false;
            }
        }
        if (allEqual && values.Count > 0)
        {
            return values[0];
        }
        
        return "String";
    }

    public void ExecuteEntityFunctionHero(string entityName, Action<Hero> action)
    {
        if (entities[entityName] is Hero hero) {
            action(hero);
        } else {
            throw new InvalidOperationException("Only heroes can perform this action.");
        }
    }

    public void ExecuteEntityFunctionDoor(string entityName, Action<Door> action)
    {
        if (entities[entityName] is Door door) {
            action(door);
        } else {
            throw new InvalidOperationException("Only doors can perform this action.");
        }
    }

    public static bool HasEvenBrackets(string input)
    {
        int parenCount = 0;
        int squareCount = 0;
        int curlyCount = 0;

        foreach (char c in input)
        {
            if (c == '(') parenCount++;
            else if (c == ')') parenCount--;
            else if (c == '[') squareCount++;
            else if (c == ']') squareCount--;
            else if (c == '{') curlyCount++;
            else if (c == '}') curlyCount--;

            if (parenCount < 0 || squareCount < 0 || curlyCount < 0) return false;
        }

        return parenCount == 0 && squareCount == 0 && curlyCount == 0;
    }

}
