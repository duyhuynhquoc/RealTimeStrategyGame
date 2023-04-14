using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField]
    private RectTransform unitSelectionArea = null;

    [SerializeField]
    private LayerMask layerMask = new LayerMask();

    private RTSPlayer player;

    private Vector2 startPosition;

    private Camera mainCamera;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void Update()
    {
        if (player == null && NetworkClient.connection.identity != null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        // If we are pressing shift, do not clear the current selection
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
        }

        // Active the selection UI image
        unitSelectionArea.gameObject.SetActive(true);

        // Set start position of selection area
        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        // Update the size and position of the selection area
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition =
            startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        // Deactivate the selection UI image
        unitSelectionArea.gameObject.SetActive(false);

        // If is clicking on a unit, select it
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            SelectASingleUnit();
            return;
        }

        // If is dragging the mouse, select all units within the selection area
        SelectMultipleUnits();
    }

    private void SelectMultipleUnits()
    {
        foreach (Unit unit in player.GetMyUnits())
        {
            // If the unit is already selected, do not select it again
            if (SelectedUnits.Contains(unit))
            {
                continue;
            }

            if (IsWithinSelectionBounds(unit.gameObject))
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private bool IsWithinSelectionBounds(GameObject gameObject)
    {
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(gameObject.transform.position);

        // Compare the position of the unit with the position of the selection area
        return screenPosition.x > min.x
            && screenPosition.x < max.x
            && screenPosition.y > min.y
            && screenPosition.y < max.y;
    }

    private void SelectASingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return;
        }

        if (!hit.collider.TryGetComponent<Unit>(out Unit unit))
        {
            return;
        }

        if (!unit.isOwned)
        {
            return;
        }

        SelectedUnits.Add(unit);

        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Select();
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }
}
