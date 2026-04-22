using UnityEngine;

namespace New_Scripts.UI
{
    // Hedef UI objesi üzerindeki Animator bileşenini kullanarak, verilen indeks değerine göre animasyon durumlarını tetikleyen kontrolcü sınıftır.
    public class UIAnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private UIAnimations UIIndex = UIAnimations.Empty;
        
        private readonly int animationIndexHash = Animator.StringToHash("index");

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            animator.SetInteger(animationIndexHash, (int)UIIndex);
        }
    }
}

public enum UIAnimations
{
    Empty = -1,
    LS_Left_Rigth = 0,
    LS_Up_Down = 1,
    LS_All = 2,
    RS_All = 3,
    A_Button = 4,
    X_Button = 5,
    LB_Button = 6,
    RB_Button = 7,
    LT_Button = 8,
    RT_Button = 9,
}