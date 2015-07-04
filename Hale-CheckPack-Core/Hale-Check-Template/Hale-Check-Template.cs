using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hale.Agent
{
    /// <summary>
    /// This is a mandatory class that should contain all information regarding the check. This will be instantiated and added to the dynamic list in the Agent.
    /// </summary>
    public class Check
    {

        /// <summary>
        /// The name of the check. This will be visible in the Web UI.
        /// For example: "System Uptime"
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Person and organization (opt) that developed the check.
        /// </summary>
        public string Author
        {
            get;
            set;
        }

        /// <summary>
        /// Internal version of the check itself.
        /// </summary>
        public decimal Version
        {
            get;
            set;
        }

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform
        {
            get;
            set;
        }

        /// <summary>
        /// What Hale Core was this check developed for?
        /// This is to avoid compability issues.
        /// </summary>
        public decimal TargetCore
        {
            get;
            set;
        }

        /// <summary>
        /// Set all the attributes above directly in the constructor.
        /// </summary>
        public Check()
        {
            Name = "Template";
            Author = "Simon Aronsson, It's Hale";
            Platform = "N/A";
            Version = 0.01M;
            TargetCore = 0.01M;
        }


        /// <summary>
        /// Executes the check and returns a Response instance. This is then serialized in
        /// the agent to JSON. This method must always be named Execute, and should always return a
        /// response following the stated standard. However, you are free to add as much logic as your
        /// use case requires in other classes/methods.
        /// 
        /// Any checks not adhering to this standard will not be merged into the checkpacks.
        /// </summary>
        /// <param name="crit">Not used for this check, but mandatory</param>
        /// <param name="warn">Not used for this check, but mandatory</param>
        /// <param name="origin">The host requesting the check</param>
        public Response Execute(string origin, long warn = 0, long crit = 0 )
        {
            Response response = new Response();
            
            // Add logic here and populate response.

            return response;
        }
    }
}
