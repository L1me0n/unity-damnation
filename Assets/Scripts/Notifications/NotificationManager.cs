using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;


public class NotificationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationParent;
    [SerializeField] private DeploymentSelection deploymentSelection;

    [Header("Settings")]
    [SerializeField] private int maxNotifications = 3;

    private Queue<Notification> availableNotifications = new Queue<Notification>();
    private List<Notification> activeNotifications = new List<Notification>();

    private void OnEnable()
    {
        deploymentSelection.OnDeploymentBlocked += ShowDeploymentBlockedNotification;
    }

    private void OnDisable()
    {
        deploymentSelection.OnDeploymentBlocked -= ShowDeploymentBlockedNotification;
    }

    private void Awake()
    {
        if (notificationPrefab == null)
        {
            Debug.LogWarning("No reference for Notification Prefab");
        }

        if (notificationParent == null)
        {
            Debug.LogWarning("No reference for Notification Parent");
        }

        if (deploymentSelection == null)
        {
            deploymentSelection = FindObjectOfType<DeploymentSelection>();
        }
    }

    private void ShowDeploymentBlockedNotification()
    {
        Notification notification;
        if (activeNotifications.Count >= maxNotifications)
        {
            notification = activeNotifications[0];
            activeNotifications.RemoveAt(0);
            notification.OnNotificationFinished -= HandleNotificationFinished;
            notification.HideNotification();
            
        }
        else if (availableNotifications.Count > 0)
        {
            notification = availableNotifications.Dequeue();
        }
        else
        {
            notification = Instantiate(notificationPrefab, notificationParent).GetComponent<Notification>();
        }

        notification.transform.SetAsFirstSibling();

        activeNotifications.Add(notification);

        notification.OnNotificationFinished -= HandleNotificationFinished;
        notification.OnNotificationFinished += HandleNotificationFinished;

        notification.FillNotificationText("Please select a different node!");
    }

    private void HandleNotificationFinished(Notification notification)
    {
        notification.OnNotificationFinished -= HandleNotificationFinished;
        activeNotifications.Remove(notification);
        availableNotifications.Enqueue(notification);
        notification.HideNotification();
    }
}
