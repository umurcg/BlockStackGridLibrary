using System;
using System.Collections.Generic;
using DG.DemiEditor;
using ObjectType;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BlockStackGridLibrary
{
    [InitializeOnLoad]
    public class LevelEditorPanel
    {
        public enum LogMode
        {
            None,
            Selected,
            AllCells
        }

        private static LogMode logMode = LogMode.Selected;
        private static string typeToInsert;
        private static int numberOfItemsToInsert = 0;


        static LevelEditorPanel()
        {
            // Register the callback to draw the GUI in the Scene view
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var levelEditor = Object.FindObjectOfType<LevelCreator>();

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (levelEditor && levelEditor.level && levelEditor.IsDirty())
                {
                    bool continuePlay = EditorUtility.DisplayDialog(
                        "Unsaved Level",
                        "You have unsaved changes. If you enter play mode, all progress will be lost. Do you want to continue?",
                        "Yes",
                        "No"
                    );

                    if (!continuePlay)
                    {
                        EditorApplication.isPlaying = false;
                        return;
                    }
                }

                // Clear level board before play
                if (levelEditor != null)
                {
                    levelEditor.ClearBoard();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (levelEditor != null)
                {
                    levelEditor.LoadLevel();
                }
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var levelEditor = Object.FindObjectOfType<LevelCreator>();

            if (levelEditor == null)
            {
                return;
            }

            if (levelEditor.level == null) return;

            Handles.BeginGUI();

            // Create a rectangle for the stats box
            Rect rect = new Rect(0, 10, 250, 700);
            GUILayout.BeginArea(rect, EditorStyles.helpBox);

            GUILayout.Label("LevelEditor", EditorStyles.boldLabel);


            List<LevelCellEditor> selectedCells = new List<LevelCellEditor>();
            LevelCellEditor[] allCells = levelEditor.GetComponentsInChildren<LevelCellEditor>();

            // Get the currently selected GameObjects
            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject obj in selectedObjects)
            {
                LevelCellEditor cellEditor = obj.GetComponent<LevelCellEditor>();
                if (cellEditor != null)
                {
                    selectedCells.Add(cellEditor);
                }
            }

            LogStats(selectedCells, allCells, levelEditor);

            if (selectedCells.Count == 2)
            {
                //Create switch button
                if (GUILayout.Button("Switch"))
                {
                    var data1 = selectedCells[0].levelCellData;
                    var data2 = selectedCells[1].levelCellData;
                    selectedCells[0].levelCellData = data2;
                    selectedCells[1].levelCellData = data1;
                    selectedCells[0].UpdateSlot();
                    selectedCells[1].UpdateSlot();
                }
            }

            DrawSlotTool(selectedCells);

            DrawSetters(selectedCells);
            DrawSelectors(allCells);

            DrawMainButtons(levelEditor);


            GUILayout.EndArea();
            Handles.EndGUI();

            sceneView.Repaint();
        }

        private static void LogStats(List<LevelCellEditor> selectedCells, LevelCellEditor[] allCells,
            LevelCreator levelCreator)
        {
            // Dropdown for log mode
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Log Mode:");
            logMode = (LogMode)EditorGUILayout.EnumPopup(logMode);
            EditorGUILayout.EndHorizontal();

            List<LevelCellEditor> cellsToCount = new List<LevelCellEditor>();

            if (logMode == LogMode.Selected)
            {
                cellsToCount.AddRange(selectedCells);
            }
            else if (logMode == LogMode.AllCells)
            {
                cellsToCount.AddRange(allCells);
            }

            Dictionary<string, int> colorCount = new Dictionary<string, int>();
            // Display each selected GameObject's name
            foreach (var cell in cellsToCount)
            {
                var data = cell.levelCellData;
                if (data.cellType == CellTypes.Slot)
                {
                    if (data.blockStackData.subStacks == null) continue;

                    foreach (var block in data.blockStackData.subStacks)
                    {
                        colorCount.TryAdd(block.TypeName, 0);
                        colorCount[block.TypeName] += block.numberOfStack;
                    }
                }
            }

            foreach (var color in colorCount)
            {
                GUILayout.Label($"{color.Key.ToString()}: {color.Value}");
            }

            GUI.color = Color.white;
            GUILayout.Label($"Selected Cell Count: {selectedCells.Count}");
            GUILayout.Label("Assigned Level: " + levelCreator.level.name);
        }


        private static void DrawMainButtons(LevelCreator levelCreator)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Main Buttons", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Board"))
            {
                levelCreator.level = null;
                levelCreator.ClearBoard();
            }

            if (GUILayout.Button("Save Level"))
            {
                levelCreator.SaveLevel();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Level"))
            {
                levelCreator.LoadLevel();
            }

            if (GUILayout.Button("Play Level"))
            {
                levelCreator.PlayLevel();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Select Level"))
            {
                Selection.activeObject = levelCreator.level;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static void DrawSelectors(LevelCellEditor[] allCells)
        {
            //Create a sub panel for Setters
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Selectors", EditorStyles.boldLabel);

            //Begin horizontal layout for buttons
            GUILayout.BeginHorizontal();

            //Loop on CellType Enum
            foreach (CellTypes cellType in System.Enum.GetValues(typeof(CellTypes)))
            {
                if (GUILayout.Button($"{cellType}"))
                {
                    List<GameObject> objectsToSelect = new List<GameObject>();
                    foreach (var cell in allCells)
                    {
                        if (cell.levelCellData.cellType == cellType)
                        {
                            objectsToSelect.Add(cell.gameObject);
                        }
                    }

                    Selection.objects = objectsToSelect.ToArray();
                }
            }

            //End horizontal layout
            GUILayout.EndHorizontal();
            //End vertical layout
            GUILayout.EndVertical();
        }

        private static void DrawSetters(List<LevelCellEditor> selectedCells)
        {
            //Create a sub panel for Setters
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Setters", EditorStyles.boldLabel);

            //Begin horizontal layout for buttons
            GUILayout.BeginHorizontal();

            //Loop on CellType Enum
            foreach (CellTypes cellType in System.Enum.GetValues(typeof(CellTypes)))
            {
                if (GUILayout.Button($"{cellType}"))
                {
                    foreach (var cell in selectedCells)
                    {
                        cell.levelCellData.cellType = cellType;
                        cell.UpdateSlot();
                    }
                }
            }

            //End horizontal layout
            GUILayout.EndHorizontal();
            //End vertical layout
            GUILayout.EndVertical();
        }

        private static void ReverseSubStacks(List<LevelCellEditor> selectedCells)
        {
            Undo.RecordObjects(selectedCells.ToArray(), "Reverse SubStacks");

            foreach (var cell in selectedCells)
            {
                if (cell.levelCellData.cellType == CellTypes.Slot)
                {
                    var data = cell.levelCellData;
                    var subStacks = data.blockStackData.subStacks;
                    for (int i = 0; i < subStacks.Length / 2; i++)
                    {
                        (subStacks[i], subStacks[subStacks.Length - i - 1]) =
                            (subStacks[subStacks.Length - i - 1], subStacks[i]);
                    }

                    cell.UpdateSlot();
                    EditorUtility.SetDirty(cell);
                }
            }
        }


        private static void ShuffleSubStacks(List<LevelCellEditor> selectedCells)
        {
            Undo.RecordObjects(selectedCells.ToArray(), "Shuffle SubStacks");

            foreach (var cell in selectedCells)
            {
                if (cell.levelCellData.cellType == CellTypes.Slot)
                {
                    var data = cell.levelCellData;
                    var subStacks = data.blockStackData.subStacks;
                    for (int i = 0; i < subStacks.Length; i++)
                    {
                        var randomIndex = Random.Range(0, subStacks.Length);
                        (subStacks[i], subStacks[randomIndex]) = (subStacks[randomIndex], subStacks[i]);
                    }

                    cell.UpdateSlot();
                    EditorUtility.SetDirty(cell);
                }
            }
        }

        private static void DrawSlotTool(List<LevelCellEditor> selectedCells)
        {
            //Vertical Layout for Insert Tool

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Slot Tool", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            //Draw ItemInsertTool
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type:");

            var types = ObjectTypeLibrary.Find().GetObjectTypeNames();
            int typeIndex = Array.IndexOf(types, typeToInsert);
            typeIndex = EditorGUILayout.Popup(typeIndex, types);
            if (typeIndex >= 0)
            {
                typeToInsert = types[typeIndex];
            }


            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Count:");

            numberOfItemsToInsert = EditorGUILayout.IntField(numberOfItemsToInsert);
            EditorGUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Insert Items"))
            {
                InsertItemsToSlots(selectedCells);
            }

            if (GUILayout.Button("Subtract Item"))
            {
                SubtractItem(selectedCells);
            }

            if (GUILayout.Button("Clear Slots"))
            {
                foreach (var cell in selectedCells)
                {
                    if (cell.levelCellData.cellType == CellTypes.Slot)
                    {
                        cell.levelCellData.blockStackData = new BlockStackData();
                        cell.UpdateSlot();
                    }
                }
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Color Tools", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Shuffle Colors"))
            {
                ShuffleSubStacks(selectedCells);
            }

            if (GUILayout.Button("Reverse Colors"))
            {
                ReverseSubStacks(selectedCells);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private static void SubtractItem(List<LevelCellEditor> selectedCells)
        {
            foreach (var cell in selectedCells)
            {
                if (cell.levelCellData.cellType == CellTypes.Slot)
                {
                    if (cell.levelCellData.blockStackData.subStacks == null ||
                        cell.levelCellData.blockStackData.subStacks.Length == 0)
                    {
                        continue;
                    }

                    cell.levelCellData.blockStackData.subStacks[^1].numberOfStack--;
                    if (cell.levelCellData.blockStackData.subStacks[^1].numberOfStack == 0)
                    {
                        var newSubStacks = new BlockSubStack[cell.levelCellData.blockStackData.subStacks.Length - 1];
                        for (int i = 0; i < newSubStacks.Length; i++)
                        {
                            newSubStacks[i] = cell.levelCellData.blockStackData.subStacks[i];
                        }

                        cell.levelCellData.blockStackData.subStacks = newSubStacks;
                    }

                    cell.UpdateSlot();
                }
            }
        }

        private static void InsertItemsToSlots(List<LevelCellEditor> selectedCells)
        {
            List<LevelCellEditor> slotCells = new List<LevelCellEditor>();
            foreach (var cell in selectedCells)
            {
                if (cell.levelCellData.cellType == CellTypes.Slot)
                {
                    slotCells.Add(cell);
                }
            }

            if (slotCells.Count == 0)
            {
                Debug.LogError("No slot selected");
            }
            else
            {
                int insertedItemCount = 0;

                // Register undo for the current state of each cell
                foreach (var cell in slotCells)
                {
                    Undo.RecordObject(cell, "Insert Items");
                }

                //Shuffle the list of cells
                slotCells.Shuffle();


                //First add one item to all slots
                foreach (var cell in slotCells)
                {
                    if (insertedItemCount >= numberOfItemsToInsert) break;

                    var data = cell.levelCellData;
                    if (data.blockStackData.subStacks == null)
                        data.blockStackData.subStacks = Array.Empty<BlockSubStack>();

                    if (data.blockStackData.subStacks.Length == 0 ||
                        data.blockStackData.subStacks[^1].TypeName != typeToInsert)
                    {
                        var newSubStack = new BlockSubStack();
                        newSubStack.SetType(typeToInsert);
                        newSubStack.numberOfStack = 1;
                        var newSubStacks = new BlockSubStack[data.blockStackData.subStacks.Length + 1];
                        for (int i = 0; i < data.blockStackData.subStacks.Length; i++)
                        {
                            newSubStacks[i] = data.blockStackData.subStacks[i];
                        }

                        newSubStacks[^1] = newSubStack;
                        data.blockStackData.subStacks = newSubStacks;
                    }
                    else
                    {
                        data.blockStackData.subStacks[^1].numberOfStack++;
                    }

                    insertedItemCount++;
                }

                //Then add the rest of the items to random slots
                while (insertedItemCount < numberOfItemsToInsert)
                {
                    var randomIndex = Random.Range(0, slotCells.Count);
                    var cell = slotCells[randomIndex];
                    var data = cell.levelCellData;
                    data.blockStackData.subStacks[^1].numberOfStack++;
                    insertedItemCount++;
                }

                // Mark each cell as dirty to ensure changes are saved
                foreach (var cell in slotCells)
                {
                    cell.UpdateSlot();
                    EditorUtility.SetDirty(cell);
                }
            }
        }
    }
}