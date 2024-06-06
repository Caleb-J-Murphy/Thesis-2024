using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    private Dictionary<string, Action<string>> entityFunctions;
    private Dictionary<string, Entity> entities;

    private Hero Hero;

    private Board board;
    public float stepDelay = 1f; // Time delay between each step, adjustable in the inspector

    private GameController gameController;
    void Start()
    {
        gameController = GetComponent<GameController>();
        if (gameController == null) {
            return;
        }
        string map = @"WWWW
                       WM W
                       WH W
                       WWWW";
        gameController.Initialise(map, out board, out entities, out entityFunctions);
        
    
        string input = @"
            while(!hero.isTouchingWall()) {
                hero.moveForward(1)
                if(!hero.isTouchingWall()) {
                    hero.moveForward(2)
                }
            }
            hero.moveForward(1)
            hero.moveDown()
            hero.moveUp()
        ";

        StartCoroutine(ProcessInput(input));
    }

    

    private IEnumerator ProcessInput(string input)
    {
        //Check to see if there is an even number of [], {}, and ()
        if (!HasEvenBrackets(input)) {
            Debug.LogError("You have a miss matched bracket");
            yield return null;
        }
        var lines = input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        yield return StartCoroutine(ExecuteLines(lines, 0));
    }

    public static bool HasEvenBrackets(string input)
    {
        int parenCount = 0;
        int squareCount = 0;
        int curlyCount = 0;

        foreach (char c in input)
        {
            if (c == '(')
            {
                parenCount++;
            }
            else if (c == ')')
            {
                parenCount--;
            }
            else if (c == '[')
            {
                squareCount++;
            }
            else if (c == ']')
            {
                squareCount--;
            }
            else if (c == '{')
            {
                curlyCount++;
            }
            else if (c == '}')
            {
                curlyCount--;
            }

            if (parenCount < 0 || squareCount < 0 || curlyCount < 0)
            {
                return false; // More closing brackets than opening brackets
            }
        }

        return parenCount == 0 && squareCount == 0 && curlyCount == 0;
    }

    private IEnumerator ExecuteLines(string[] lines, int depth)
    {
        List<string> currentBlock = new List<string>();
        bool inControlFlow = false;
        string controlExpression = "";
        string controlType = "";
        int lineNumber = 0;

        while (lineNumber < lines.Length)
        {
            var trimmedLine = lines[lineNumber].Trim();

            if (IsControlFlowStart(trimmedLine, out controlType, out controlExpression))
            {
                inControlFlow = true;
                //Now we go through each of the values inside the block to wait until we get to the end }, if we find 1 that is { then we add 1 to the recursion list
                int recursionCheck = 0;
                lineNumber++;
                while (lineNumber < lines.Length) {
                    trimmedLine = lines[lineNumber].Trim();
                    if (trimmedLine == "{") {
                        recursionCheck++;
                    } else if (trimmedLine == "}") {
                        if (recursionCheck > 0) {
                            recursionCheck--;
                        } else {
                            break;
                        }
                    }
                    currentBlock.Add(trimmedLine);
                    lineNumber++;
                    
                }
                yield return StartCoroutine(HandleControlFlow(controlType, controlExpression, currentBlock.ToArray(), depth + 1));
            }
            yield return StartCoroutine(ExecuteLine(trimmedLine));
            lineNumber++;
        }
    }

    private bool IsControlFlowStart(string line, out string controlType, out string controlExpression)
    {
        controlType = null;
        controlExpression = null;

        if (line.StartsWith("while("))
        {
            controlType = "while";
            controlExpression = ExtractExpression(line);
            return true;
        }
        if (line.StartsWith("if("))
        {
            controlType = "if";
            controlExpression = ExtractExpression(line);
            return true;
        }

        return false;
    }

    private IEnumerator ExecuteLine(string line)
    {
        var parts = line.Split(new[] { '.' }, 2);
        if (parts.Length < 2) yield break;

        var entityName = parts[0];
        var commandPart = parts[1].Split(new[] { '(', ')' });

        //if (commandPart.Length < 2) yield break;

        var commandName = commandPart[0];
        var param = commandPart[1];

        if (entities.ContainsKey(entityName) && entityFunctions.ContainsKey(commandName))
        {
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
        if (depth > 200)
        {
            Debug.LogError("TOO MANY LOOPS");
            yield break;
        }

        switch (controlType)
        {
            case "while":
                while (EvaluateExpression(expression))
                {
                    Debug.Log("Starting a new while");
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

            var parts = expression.Split(new[] { '.' }, 2);
            if (parts.Length < 2)
            {
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

    private string ExtractExpression(string line)
    {
        int startIndex = line.IndexOf('(') + 1;
        int endIndex = line.IndexOf(')');
        return line.Substring(startIndex, endIndex - startIndex);
    }

    public void ExecuteEntityFunction(string entityName, Action<Hero> action)
    {
        if (entities[entityName] is Hero hero)
        {
            action(hero);
        }
        else
        {
            throw new InvalidOperationException("Only heroes can perform this action.");
        }
    }
}
