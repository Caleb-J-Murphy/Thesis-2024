using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class InputProcessor : MonoBehaviour
{
    public TMP_InputField textMeshProInputField;

    private Dictionary<string, Action<string>> entityFunctions;
    private Dictionary<string, Entity> entities;
    private Hero Hero;
    private Board board;
    public float stepDelay = 1f; // Time delay between each step, adjustable in the inspector
    private GameController gameController;

    private string input = "";
    private string map = "";

    private Dictionary<string, object> variableValues = new Dictionary<string, object>();

    [SerializeField] private int maxWhileLoop = 10;
    [SerializeField] private int maxLoopDepth = 10;
    private bool stopRequested = false; // Flag to stop execution

    [SerializeField] private bool breakWhile = false;



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
    }

    public void StartExecution()
    {
        input = textMeshProInputField.text;
        stopRequested = false; // Reset the stop flag
        StartCoroutine(ExecuteSequentially(input));

    }

    public void StopExecution()
    {
        stopRequested = true; // Set the stop flag
    }

    

    private IEnumerator Reset()
    {
        Debug.Log("Running Reset");
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
                yield break;
            }


            if (IsControlFlowStart(trimmedLine, out string controlType, out string controlExpression))
            {
                int recursionCheck = 0;
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
                        if (recursionCheck > 0)
                        {
                            recursionCheck--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    currentBlock.Add(trimmedLine);
                    lineNumber++;
                }
                yield return StartCoroutine(HandleControlFlow(controlType, controlExpression, currentBlock.ToArray(), depth + 1));
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
            }
            else
            {
                yield return StartCoroutine(ExecuteLine(trimmedLine));
            }
            lineNumber++;
        }
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
        if (line.StartsWith("while") || line.StartsWith("if"))
        {
            if (line.StartsWith("while("))
            {
                controlType = "while";
                controlExpression = ExtractExpression(line);
                return true;
            } else if (line.StartsWith("if("))
            {
                controlType = "if";
                controlExpression = ExtractExpression(line);
                return true;
            } else
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

        switch (controlType)
        {
            case "while":
                int loopCount = 0;
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
                break;

            case "if":
                if (EvaluateExpression(expression))
                {
                    yield return StartCoroutine(ExecuteLines(steps, depth));
                    yield return new WaitForSeconds(stepDelay);
                }
                break;

            default:
                Debug.LogError($"Unsupported control type: {controlType}");
                break;
        }
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
        // Add more types if needed
        Debug.LogError($"No value value able to be associated with {part}");
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
                if (int.TryParse(leftValue.ToString(),out int lValue) && int.TryParse(rightValue.ToString(), out int rValue))
                {
                    return lValue == rValue;
                }
                return leftValue.Equals(rightValue);
            case "!=":
                if (int.TryParse(leftValue.ToString(), out int lsValue) && int.TryParse(rightValue.ToString(), out int rsValue))
                {
                    return lsValue != rsValue;
                }
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
        int endIndex = line.IndexOf(')');
        return line.Substring(startIndex, endIndex - startIndex);
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

    private object EvaluateValue(string varType, string value)
    {
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
                    value = (leftValue - rightValue).ToString();
                    break;
                case "+":
                    value = (leftValue + rightValue).ToString();
                    break;
                case "*":
                    value = (leftValue * rightValue).ToString();
                    break;
                case "/":
                    value = (leftValue / rightValue).ToString();
                    break;
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
