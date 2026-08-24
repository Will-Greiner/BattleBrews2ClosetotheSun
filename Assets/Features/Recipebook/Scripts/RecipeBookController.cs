// using UnityEngine;

// public class RecipeBookController : MonoBehaviour, IHandInteractable
// {

//     [Header("View")]
//     [SerializeField] private Transform bookViewPoint;

//     [Header("Cover Pivots")]
//     [SerializeField] private Transform leftCoverPivot;
//     [SerializeField] private Transform rightCoverPivot;
//     [SerializeField] private Vector3 leftCoverOpenLocalEuler;
//     [SerializeField] private Vector3 rightCoverOpenLocalEuler;

//     [Header("Page UI")]
//     [SerializeField] private GameObject leftPageCanvas;
//     [SerializeField] private GameObject rightPageCanvas;

//     [Header("Movement")]
//     [Min(0.01f)] [SerializeField] private float moveToViewDuration = 0.6f;
//     [Min(0.01f)] [SerializeField] private float returnDuration = 0.6f;
//     [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

//     [Header("Opening")]
//     [Min(0.01f)] [SerializeField] private float coverAnimationDuration = 0.45f;
//     [SerializeField] private AnimationCurve coverAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

//     [Header("Prompt")]
//     [SerializeField] private string openPrompt = "Read Recipe Book";

//     private GrabController activeGrabController;
//     private Coroutine transitionRoutine;
//     private Transform pedestalParent;
//     private Vector3 pedestalLocalPosition;
//     private Quaternion pedestalLocalRotation;
//     private Vector3 pedestalLocalScale;
//     private Quaternion leftCoverClosedRotation;
//     private Quaternion rightCoverClosedRotation;
//     private bool isOpen;
//     private bool isTransitioning;

//     public bool IsOpen => isOpen;
//     public bool IsTransitioning => isTransitioning;


//     private void Awake()
//     {
//         pedestalParent = transform.parent;
//         pedestalLocalPosition = transform.localPosition;
//         pedestalLocalRotation = transform.localRotation;
//         pedestalLocalScale = transform.localScale;

//         if (leftCoverPivot != null)
//             leftCoverClosedRotation = leftCoverPivot.localRotation;

//         if (rightCoverPivot != null)
//             rightCoverClosedRotation = rightCoverPivot.localRotation;

//         SetPageCanvasesVisible(false);
//     }


//     public bool CanInteract(GrabController grabController)
//     {
//         return !isOpen && !isTransitioning && grabController != null && !grabController.IsHoldingItem;
//     }

//     public void Interact(GrabController grabController)
//     {
//         if (CanInteract(grabController))
//             OpenBook(grabController);
//     }

//     public string GetInteractionPrompt(GrabController grabController)
//     {
//         return CanInteract(grabController) ? openPrompt : string.Empty;
//     }

//     public void OpenBook(GrabController grabController)
//     {
//         if (!CanInteract(grabController) || bookViewPoint == null)
//             return;

//         activeGrabController = grabController;

//         if (transitionRoutine != null)
//         StopCoroutine(transitionRoutines);
//     }
// }
