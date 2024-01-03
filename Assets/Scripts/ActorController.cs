using System;

using DG.Tweening;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.InputSystem;

public class ActorController : MonoBehaviour {
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform weapon;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Control")] private Transform transform;

    private Rigidbody rigidbody;
    private Vector2 moveInput;
    [SerializeField] private float velocity;

    private Transform camera;
    private Vector2 lookInput;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Vector2 rotationClamp = new Vector2(-60f, 60f);
    private Vector3 cameraRotation;

    private bool attack;
    private bool isAttacking;

    private void Awake() {
        transform = GetComponent<Transform>();
        camera = Camera.main.GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() { }

    private void FixedUpdate() {
        OnLocomotion();
    }

    // Update is called once per frame
    private void Update() {
        OnInput();
        OnAttack();

        // 临时隐藏鼠标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnInput() {
        moveInput = Vector2.zero;
        moveInput.y += Keyboard.current.wKey.isPressed ? 1f : 0f;
        moveInput.y -= Keyboard.current.sKey.isPressed ? 1f : 0f;
        moveInput.x += Keyboard.current.dKey.isPressed ? 1f : 0f;
        moveInput.x -= Keyboard.current.aKey.isPressed ? 1f : 0f;

        lookInput = Mouse.current.delta.ReadValue();
        attack = Mouse.current.leftButton.isPressed;
    }

    private void OnLocomotion() {
        cameraRotation += new Vector3(-lookInput.y, lookInput.x, 0) * (rotationSpeed * Time.fixedDeltaTime);
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, rotationClamp.x, rotationClamp.y);
        camera.localRotation = Quaternion.Euler(cameraRotation.x, 0f, 0f);

        rigidbody.velocity = (transform.forward * moveInput.y + transform.right * moveInput.x) * velocity;
        transform.localRotation = Quaternion.Euler(0f, cameraRotation.y, 0f);
    }

    private void OnAttack() {
        if (isAttacking || !attack) {
            return;
        }

        var startPos = weapon.localPosition;
        isAttacking = true;
        trailRenderer.enabled = true;
        weapon.DOLocalMove(startPos + 1.5f * Vector3.left, 0.15f).OnComplete(() => weapon.DOLocalMove(startPos, 0.5f).SetDelay(0.5f).OnComplete(() => {
            isAttacking = false;
            trailRenderer.enabled = false;
        }));

        if (Physics.Raycast(camera.position, camera.forward, out var hitInfo, 3, LayerMask.GetMask("Default"))) {
            MeshManager.Instance.MeshSlice(hitInfo.transform, hitInfo.point, transform.right, transform.forward);
        }
    }
}