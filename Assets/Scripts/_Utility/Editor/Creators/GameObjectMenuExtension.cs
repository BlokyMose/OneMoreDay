using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using Encore.Interactables;
using Encore.MiniGames.Viewers;
using Encore.Inventory;

public class GameObjectMenuExtension : MonoBehaviour
{
    [MenuItem("GameObject/Viewer/Image Viewer (Interactable)",false,0)]
    public static void CreateImageViewer()
    {
        var imageViewerCanvasPrefabGUIDs = AssetDatabase.FindAssets("ImageViewerCanvas");
        if(imageViewerCanvasPrefabGUIDs.Length == 0)
        {
            Debug.Log("Cannot find ImageViewerCanvas prefab");
            return;
        }
        var imageViewerCanvasPrefabPath = AssetDatabase.GUIDToAssetPath(imageViewerCanvasPrefabGUIDs[0]);

        var imageViewerParent = new GameObject("ImageViewer (Interactable)");
        var goActivator = imageViewerParent.AddComponent<GOActivator>();
        var relay = imageViewerParent.AddComponent<RelayAction>();
        var spriteRenderer = imageViewerParent.AddComponent<SpriteRenderer>();
        goActivator.AddCapsuleCol();

        var imageViewerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(imageViewerCanvasPrefabPath);
        var imageViewerCanvasGO = Instantiate(imageViewerCanvasPrefab, imageViewerParent.transform);
        var imageViewer = imageViewerCanvasGO.GetComponent<ImageViewer>();

        goActivator.goActivatedItems.Add(new GOActivator.GOActivatedItem(
            go: imageViewerCanvasGO,
            highlightSpriteRenderer: spriteRenderer,
            activationBeforeInteraction: GOActivator.GOActivatedItem.GOActivationMode.Inactive,
            activationAfterInteraction: GOActivator.GOActivatedItem.GOActivationMode.Active
            ));

        UnityEventTools.AddBoolPersistentListener(relay.ExecuteEvent, imageViewer.Show, true);
        imageViewerCanvasGO.SetActive(false);

        Selection.activeGameObject = imageViewerCanvasGO;
        EditorGUIUtility.PingObject(imageViewerCanvasGO);
    }

    [MenuItem("GameObject/Viewer/Image Viewer Pickupable (Interactable)", false, 0)]
    public static void CreateImageViewerPickupable()
    {
        var imageViewerCanvasPrefabGUIDs = AssetDatabase.FindAssets("ImageViewerCanvas");
        if (imageViewerCanvasPrefabGUIDs.Length == 0)
        {
            Debug.Log("Cannot find ImageViewerCanvas prefab");
            return;
        }
        var imageViewerCanvasPrefabPath = AssetDatabase.GUIDToAssetPath(imageViewerCanvasPrefabGUIDs[0]);

        var imageViewerParent = new GameObject("ImageViewer (Interactable)");
        var goActivator = imageViewerParent.AddComponent<GOActivator>();
        var relay = imageViewerParent.AddComponent<RelayAction>();
        var spriteRenderer = imageViewerParent.AddComponent<SpriteRenderer>();
        goActivator.AddCapsuleCol();
        var pickupHook = imageViewerParent.AddComponent<ItemPickupHook>();

        var imageViewerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(imageViewerCanvasPrefabPath);
        var imageViewerCanvasGO = Instantiate(imageViewerCanvasPrefab, imageViewerParent.transform);
        var imageViewer = imageViewerCanvasGO.GetComponent<ImageViewer>();
        imageViewer.PickupHook = pickupHook;

        goActivator.goActivatedItems.Add(new GOActivator.GOActivatedItem(
            go: imageViewerCanvasGO,
            highlightSpriteRenderer: spriteRenderer,
            activationBeforeInteraction: GOActivator.GOActivatedItem.GOActivationMode.Inactive,
            activationAfterInteraction: GOActivator.GOActivatedItem.GOActivationMode.Active
            ));

        UnityEventTools.AddBoolPersistentListener(relay.ExecuteEvent, imageViewer.Show, true);
        imageViewerCanvasGO.SetActive(false);

        Selection.activeGameObject = imageViewerCanvasGO;
        EditorGUIUtility.PingObject(imageViewerCanvasGO);

    }



    [MenuItem("GameObject/Viewer/Text Viewer (Interactable)", false, 0)]
    public static void CreateTextViewer()
    {
        var textViewerCanvasPrefabGUIDs = AssetDatabase.FindAssets("TextViewerCanvas");
        if (textViewerCanvasPrefabGUIDs.Length == 0)
        {
            Debug.Log("Cannot find TextViewerCanvas prefab");
            return;
        }
        var textViewerCanvasPrefabPath = AssetDatabase.GUIDToAssetPath(textViewerCanvasPrefabGUIDs[0]);

        var textViewerParent = new GameObject("TextViewer (Interactable)");
        var goActivator = textViewerParent.AddComponent<GOActivator>();
        var relay = textViewerParent.AddComponent<RelayAction>();
        var spriteRenderer = textViewerParent.AddComponent<SpriteRenderer>();
        goActivator.AddCapsuleCol();

        var textViewerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(textViewerCanvasPrefabPath);
        var textViewerCanvasGO = Instantiate(textViewerCanvasPrefab, textViewerParent.transform);
        var textViewer = textViewerCanvasGO.GetComponent<TextViewer>();

        goActivator.goActivatedItems.Add(new GOActivator.GOActivatedItem(
            go: textViewerCanvasGO,
            highlightSpriteRenderer: spriteRenderer,
            activationBeforeInteraction: GOActivator.GOActivatedItem.GOActivationMode.Inactive,
            activationAfterInteraction: GOActivator.GOActivatedItem.GOActivationMode.Active
            ));

        UnityEventTools.AddBoolPersistentListener(relay.ExecuteEvent, textViewer.Show, true);
        textViewerCanvasGO.SetActive(false);

        Selection.activeGameObject = textViewerCanvasGO;
        EditorGUIUtility.PingObject(textViewerCanvasGO);

    }

    [MenuItem("GameObject/Viewer/Text Viewer Pickupable (Interactable)", false, 0)]
    public static void CreateTextViewerPickupable()
    {
        var textViewerCanvasPrefabGUIDs = AssetDatabase.FindAssets("TextViewerCanvas");
        if (textViewerCanvasPrefabGUIDs.Length == 0)
        {
            Debug.Log("Cannot find TextViewerCanvas prefab");
            return;
        }
        var textViewerCanvasPrefabPath = AssetDatabase.GUIDToAssetPath(textViewerCanvasPrefabGUIDs[0]);

        var textViewerParent = new GameObject("TextViewer (Interactable)");
        var goActivator = textViewerParent.AddComponent<GOActivator>();
        var relay = textViewerParent.AddComponent<RelayAction>();
        var spriteRenderer = textViewerParent.AddComponent<SpriteRenderer>();
        goActivator.AddCapsuleCol();
        var pickupHook = textViewerParent.AddComponent<ItemPickupHook>();

        var textViewerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(textViewerCanvasPrefabPath);
        var textViewerCanvasGO = Instantiate(textViewerCanvasPrefab, textViewerParent.transform);
        var textViewer = textViewerCanvasGO.GetComponent<TextViewer>();
        textViewer.PickupHook = pickupHook;

        goActivator.goActivatedItems.Add(new GOActivator.GOActivatedItem(
            go: textViewerCanvasGO,
            highlightSpriteRenderer: spriteRenderer,
            activationBeforeInteraction: GOActivator.GOActivatedItem.GOActivationMode.Inactive,
            activationAfterInteraction: GOActivator.GOActivatedItem.GOActivationMode.Active
            ));

        UnityEventTools.AddBoolPersistentListener(relay.ExecuteEvent, textViewer.Show, true);
        textViewerCanvasGO.SetActive(false);

        Selection.activeGameObject = textViewerCanvasGO;
        EditorGUIUtility.PingObject(textViewerCanvasGO);

    }

    public static void CreateItemActionGO(Item item)
    {
        // HARDCODE: currently ActionPrefab has to contain Viewer component which has PickupHook and Show()
        // (Feb 25, 2022)

        var itemActionParent = new GameObject(item.name);
        var goActivator = itemActionParent.AddComponent<GOActivator>();
        var relay = itemActionParent.AddComponent<RelayAction>();
        var spriteRenderer = itemActionParent.AddComponent<SpriteRenderer>();
        var pickupHook = itemActionParent.AddComponent<ItemPickupHook>();

        goActivator.SetObjectName(item.name);
        relay.SetObjectName(item.name);
        spriteRenderer.sprite = item.Sprite;
        pickupHook.Item = item;
        goActivator.AddPolygonCol();

        var itemActionGO = Instantiate(item.ActionPrefab, itemActionParent.transform);
        var viewer = itemActionGO.GetComponent<Viewer>();
        viewer.PickupHook = pickupHook;

        goActivator.goActivatedItems.Add(new GOActivator.GOActivatedItem(
            go: itemActionGO,
            highlightSpriteRenderer: spriteRenderer,
            activationBeforeInteraction: GOActivator.GOActivatedItem.GOActivationMode.Inactive,
            activationAfterInteraction: GOActivator.GOActivatedItem.GOActivationMode.Active
            ));

        UnityEventTools.AddBoolPersistentListener(relay.ExecuteEvent, viewer.Show, true);
        itemActionGO.SetActive(false);

        Selection.activeGameObject = itemActionGO;
        EditorGUIUtility.PingObject(itemActionGO);
    }
}
