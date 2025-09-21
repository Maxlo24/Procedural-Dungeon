using UnityEngine;
//#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
//#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{

        public static StarterAssetsInputs Instance { get; private set; }


        private InputAction m_FireAction;


        [Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        private bool m_FireInputWasHeld;

        //#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_FireAction = InputSystem.actions.FindAction("Player/Attack");

            m_FireAction.Enable();
        }

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
        }

        public bool CanProcessInput()
        {
			return true;
        }


        public bool GetFireInputDown()
        {
            return GetFireInputHeld() && !m_FireInputWasHeld;
        }

        public bool GetFireInputReleased()
        {
            return !GetFireInputHeld() && m_FireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                return m_FireAction.IsPressed();
            }

            return false;
        }





  //      public void OnMove(InputValue value)
		//{
		//	move = value.Get<Vector2>();
		//}

		//public void OnLook(InputValue value)
		//{
		//	if(cursorInputForLook)
		//	{
		//		look = value.Get<Vector2>();
		//	}
		//}

		//public void OnJump(InputValue value)
		//{
		//	jump = value.isPressed;
		//}

		//public void OnSprint(InputValue value)
		//{
		//	sprint = value.isPressed;
		//}

		//public void OnCrouch(InputValue value)
		//{
		//	crouch = !crouch;
		//}

		//public void OnInteract(InputValue value)
		//{
		//	interact = value.isPressed;
  //      }

		//public void OnDrop(InputValue value)
		//{
		//	drop = value.isPressed;
		//}

		//public void OnShoot(InputValue value)
		//{
		//	shoot = value.isPressed;
		//}


		//public void ScrollInput(float scrollInput)
		//{
		//	//Debug.Log(scroll);
		//	if (scrollInput < 0) scroll = -1;
		//	if (scrollInput > 0) scroll = 1;
		//	else
		//	{
		//		scroll = 0;
		//	}
		//}

  //      private void OnApplicationFocus(bool hasFocus)
		//{
		//	SetCursorState(cursorLocked);
		//}

		//public void SetCursorState(bool newState)
		//{
		//	Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		//}

		//public void SetCursorVisibility(bool visible)
		//{
		//	Cursor.visible = visible;
		//}

		//public void SetLookForMouseMove(bool lookAt)
		//{
		//	look = new Vector2(0, 0);
  //          cursorInputForLook = lookAt;
  //      }

	}
	
}