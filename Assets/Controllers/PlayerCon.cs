// GENERATED AUTOMATICALLY FROM 'Assets/Controllers/PlayerCon.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerCon : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerCon()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerCon"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""f773bafe-d7cc-4dc3-9be8-58e120b07b11"",
            ""actions"": [
                {
                    ""name"": ""Forward"",
                    ""type"": ""Value"",
                    ""id"": ""bfa9a69e-884d-49d7-acb0-5d4c8815ecbf"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""8395d501-b7ee-468e-965a-67d1382e7893"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""JumpR"",
                    ""type"": ""Button"",
                    ""id"": ""5a3595ae-3d9e-4a5b-9ba0-4acff1258479"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""JumpL"",
                    ""type"": ""Button"",
                    ""id"": ""07e398ac-b6a6-4768-b496-07c2811316ad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""JumpGap"",
                    ""type"": ""Button"",
                    ""id"": ""eb9e662e-4ec0-44a2-bd4c-f5925c731277"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Start"",
                    ""type"": ""Button"",
                    ""id"": ""2ff216fc-335c-485c-90d0-1671baf23f99"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""172d3aea-ba2d-4dbc-9ac5-7d8ab6ba6e78"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Forward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d5e63f35-34a4-4421-b7c8-f8069937a262"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8e54c8d7-6934-482a-be62-8b7b233eb6e5"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""JumpR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""65932f6c-0423-4dc4-822a-a03ec44fa59a"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""JumpL"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c2ef80b6-5a3e-4f4f-9a0a-5b24de04b5c2"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""JumpGap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f04546c7-fe2b-4e24-bdb0-108512429ca1"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""New control scheme"",
            ""bindingGroup"": ""New control scheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputControllerWindows>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Forward = m_Player.FindAction("Forward", throwIfNotFound: true);
        m_Player_Camera = m_Player.FindAction("Camera", throwIfNotFound: true);
        m_Player_JumpR = m_Player.FindAction("JumpR", throwIfNotFound: true);
        m_Player_JumpL = m_Player.FindAction("JumpL", throwIfNotFound: true);
        m_Player_JumpGap = m_Player.FindAction("JumpGap", throwIfNotFound: true);
        m_Player_Start = m_Player.FindAction("Start", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Forward;
    private readonly InputAction m_Player_Camera;
    private readonly InputAction m_Player_JumpR;
    private readonly InputAction m_Player_JumpL;
    private readonly InputAction m_Player_JumpGap;
    private readonly InputAction m_Player_Start;
    public struct PlayerActions
    {
        private @PlayerCon m_Wrapper;
        public PlayerActions(@PlayerCon wrapper) { m_Wrapper = wrapper; }
        public InputAction @Forward => m_Wrapper.m_Player_Forward;
        public InputAction @Camera => m_Wrapper.m_Player_Camera;
        public InputAction @JumpR => m_Wrapper.m_Player_JumpR;
        public InputAction @JumpL => m_Wrapper.m_Player_JumpL;
        public InputAction @JumpGap => m_Wrapper.m_Player_JumpGap;
        public InputAction @Start => m_Wrapper.m_Player_Start;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Forward.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnForward;
                @Forward.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnForward;
                @Forward.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnForward;
                @Camera.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @JumpR.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpR;
                @JumpR.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpR;
                @JumpR.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpR;
                @JumpL.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpL;
                @JumpL.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpL;
                @JumpL.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpL;
                @JumpGap.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpGap;
                @JumpGap.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpGap;
                @JumpGap.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpGap;
                @Start.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStart;
                @Start.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStart;
                @Start.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStart;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Forward.started += instance.OnForward;
                @Forward.performed += instance.OnForward;
                @Forward.canceled += instance.OnForward;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @JumpR.started += instance.OnJumpR;
                @JumpR.performed += instance.OnJumpR;
                @JumpR.canceled += instance.OnJumpR;
                @JumpL.started += instance.OnJumpL;
                @JumpL.performed += instance.OnJumpL;
                @JumpL.canceled += instance.OnJumpL;
                @JumpGap.started += instance.OnJumpGap;
                @JumpGap.performed += instance.OnJumpGap;
                @JumpGap.canceled += instance.OnJumpGap;
                @Start.started += instance.OnStart;
                @Start.performed += instance.OnStart;
                @Start.canceled += instance.OnStart;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_NewcontrolschemeSchemeIndex = -1;
    public InputControlScheme NewcontrolschemeScheme
    {
        get
        {
            if (m_NewcontrolschemeSchemeIndex == -1) m_NewcontrolschemeSchemeIndex = asset.FindControlSchemeIndex("New control scheme");
            return asset.controlSchemes[m_NewcontrolschemeSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnForward(InputAction.CallbackContext context);
        void OnCamera(InputAction.CallbackContext context);
        void OnJumpR(InputAction.CallbackContext context);
        void OnJumpL(InputAction.CallbackContext context);
        void OnJumpGap(InputAction.CallbackContext context);
        void OnStart(InputAction.CallbackContext context);
    }
}
