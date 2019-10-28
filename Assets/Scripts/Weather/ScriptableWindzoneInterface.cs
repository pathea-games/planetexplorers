    using UnityEngine;
    using System.Collections;
    using System.Reflection ;
     
    // Provides a scriptable interface to a windzone
    /* Change Log
     * 05/25/11 Created by Nividica
     *          As Unity provides no way to access a windzone via
     *              script, I am digging around in the reflections
     *              to pull out its memeber information and write
     *              getter and setter methods.
     *          Finished writing the get/set methods for everything
     *              except the Mode.
     */
    public class ScriptableWindzoneInterface : MonoBehaviour { 
        // Private vars ===================================
       
        // The windzone component
        private Component m_WindzoneComponent ;
       
        // The system qualified type of the windzone
        private System.Type m_WindzoneType ;
       
        // Used to pass an argument to WindZone setter functions
        object[] m_WindZoneArgs = new object[1] ;
       
        // Public properties ==============================
       
        // `radius`
        public float Radius {
            get
            {
                // Return the value of `radius`
                return (float)GetWindZoneValue( "get_radius" ) ;
            }
            set
            {
                // Set the argument
                m_WindZoneArgs[0] = value ;
               
                // Set the value of `windMain`
                SetWindZoneValue( "set_radius" , m_WindZoneArgs ) ;
            }
        }
       
        // `windMain`
        public float WindMain {
            get
            {
                // Return the value of `windMain`
                return (float)GetWindZoneValue( "get_windMain" ) ;
            }
            set
            {
                // Set the argument
                m_WindZoneArgs[0] = value ;
               
                // Set the value of `windMain`
                SetWindZoneValue( "set_windMain" , m_WindZoneArgs ) ;
            }
        }
       
        // `windTurbulence`
        public float WindTurbulence {
            get
            {
                // Return the value of `windTurbulence`
                return (float)GetWindZoneValue( "get_windTurbulence" ) ;
            }
            set
            {
                // Set the argument
                m_WindZoneArgs[0] = value ;
               
                // Set the value of `windTurbulence`
                SetWindZoneValue( "set_windTurbulence" , m_WindZoneArgs ) ;
            }
        }
       
        // `windPulseMagnitude`
        public float WindPulseMagnitude {
            get
            {
                // Return the value of `windPulseMagnitude`
                return (float)GetWindZoneValue( "get_windPulseMagnitude" ) ;
            }
            set
            {
                // Set the argument
                m_WindZoneArgs[0] = value ;
               
                // Set the value of `windPulseMagnitude`
                SetWindZoneValue( "set_windPulseMagnitude" , m_WindZoneArgs ) ;
            }
        }
       
        // `windPulseFrequency`
        public float WindPulseFrequency {
            get
            {
                // Return the value of `windPulseFrequency`
                return (float)GetWindZoneValue( "get_windPulseFrequency" ) ;
            }
            set
            {
                // Set the argument
                m_WindZoneArgs[0] = value ;
               
                // Set the value of `windPulseMagnitude`
                SetWindZoneValue( "set_windPulseFrequency" , m_WindZoneArgs ) ;
            }
        }
       
        // Public functions ===============================
       
        // Find and link against the windzone
        public void Start () {
           
            // Get the windzone of this game object
            m_WindzoneComponent = GetComponent("WindZone");
           
            // Ensure there was a windzone to link to
            if ( m_WindzoneComponent == null ) {
                // Log the error
                Debug.LogError( "Could not find a wind zone to link to: " + this ) ;
               
                // Disable this script for the remainder of the run
                this.enabled = false;
               
                // Stop here
                return ;
            }
           
            // Get the system qualified type
            m_WindzoneType = m_WindzoneComponent.GetType() ;
           
        }
       
        // Private functions =============================
       
        // Set a WindZone property value
        void SetWindZoneValue ( string MemberName , object[] args ){
            // Call the setter
			if ( m_WindzoneType == null || m_WindzoneComponent == null || args == null || MemberName == null )
				return;
            m_WindzoneType.InvokeMember(
                                        MemberName ,
                                        BindingFlags.InvokeMethod | BindingFlags.Instance ,
                                        null ,
                                        m_WindzoneComponent ,
                                        args ) ;
        }
       
        // Get a WindZone property value
        object GetWindZoneValue ( string MemberName ){
            // Call the getter
            return m_WindzoneType.InvokeMember(
                                               MemberName ,
                                               BindingFlags.InvokeMethod | BindingFlags.Instance ,
                                               null ,
                                               m_WindzoneComponent ,
                                               null ) ;
        }
    }