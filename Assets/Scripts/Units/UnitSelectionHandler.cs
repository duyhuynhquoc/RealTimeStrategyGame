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
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }

        SelectedUnits.Clear();

        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition =
            startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            SelectASingleUnit();
            return;
        }

        SelectMultipleUnits();
    }

    private void SelectMultipleUnits()
    {
        foreach (Unit unit in player.GetMyUnits())
        {
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
}
