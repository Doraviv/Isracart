using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Isracart.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Isracart.Controllers
{
    [ApiController]
    [Route("api/assignment")]
    public class AssignmentController : ControllerBase
    {
        #region Configurations

        /// <summary>
        /// Used to get expiration time in seconds from appsettings.json
        /// </summary>
        private readonly IOptions<SessionSettings> _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        public AssignmentController(IOptions<SessionSettings> config)
        {
            _config = config;
        }

        /// <summary>
        /// Static dictionary to save all sessions exists with their expiration time.
        /// Used to get all assignments from all sessions
        /// </summary>
        private static ConcurrentDictionary<string, SessionDictionary> sessionDictionary = new ConcurrentDictionary<string, SessionDictionary>();

        #endregion

        #region HTTP Requests

        /// <summary>
        /// Get assignments for current session
        /// </summary>
        /// <returns>assignments</returns>
        [HttpGet]
        [Route("getAssignments")]
        public JsonResult GetAssignments()
        {
            //Get session assignments
            List<Assignment> assignments = GetSessionAssignments();

            //Convert to json result object to return to the client
            var json = new JsonResult(assignments);

            return json;
        }

        /// <summary>
        /// Get all assignments from all sessions
        /// </summary>
        /// <returns>assignments from all sessions</returns>
        [HttpGet]
        [Route("getAllAssignments")]
        public JsonResult GetAllAssignments()
        {
            //Init return object
            List<Assignment> assignments = new List<Assignment>();

            foreach (var session in sessionDictionary.ToArray())
            {
                //Check if session expired
                if (session.Value.SessionExpirationDate <= DateTime.Now)
                {
                    //Remove expired sessions from the sessions dictionary
                    sessionDictionary.TryRemove(session.Key, out _);

                    //Delete expired sessions folder with its content (images)
                    DeleteDirectory(session.Key);
                }

                //Add active session's assignments to the return object
                if (session.Value != null && session.Value.Assignments != null && session.Value.Assignments.Count > 0) 
                {
                    assignments.AddRange(session.Value.Assignments);
                }
            }

            //Convert assignments to json result
            return new JsonResult(assignments);
        }

        /// <summary>
        /// Set new assignment
        /// </summary>
        /// <param name="assignmentVM">Assignments object form the view</param>
        [HttpPost]
        [Route("setAssignment")]
        public void SetAssignment([FromForm] AssignmentViewModel assignmentVM)
        {
            #region Validations

            if (string.IsNullOrWhiteSpace(assignmentVM.Description)) 
            {
                throw new ArgumentException("Assignment description must not be empty");
            }

            if (assignmentVM.File == null)
            {
                throw new ArgumentNullException("File can not be null");
            }

            #endregion

            //Save image file 
            SaveFile(assignmentVM.File);

            //Get all assignments for current session
            List<Assignment> assignments = GetSessionAssignments();

            if (assignments == null)
            {
                assignments = new List<Assignment>();
            }

            //Add the new assignment to the list
            assignments.Add(new Assignment
            {
                Id = Guid.NewGuid(),
                Name = assignmentVM.Description,
                FilePath = Path.Combine("AssignmentImages", GetSessionID(), assignmentVM.File.FileName)
            });

            //Save the modified assignment list to the session
            HttpContext.Session.SetObjectAsJson("assignments", assignments);

            //Add or update the session dictionary
            AddOrUpdateSessionDictionary(assignments);
        }

        #endregion

        #region Session Handlers

        /// <summary>
        /// Get Session ID
        /// </summary>
        /// <returns>Session ID</returns>
        private string GetSessionID()
        {
            string sessionID = HttpContext.Session.Id;
            return sessionID;
        }

        /// <summary>
        /// Add or update session Dictionary
        /// Add new session with its assignments and experation time.
        /// Update exist session with its new experation time and added assignments.
        /// Triggered when trying to get assignments from current session or adding new assignment. 
        /// </summary>
        /// <param name="assignments">NULL allowed</param>
        private void AddOrUpdateSessionDictionary(List<Assignment> assignments)
        {
            SessionDictionary sessionDictionaryToAddOrUpdate = new SessionDictionary()
            {
                Assignments = assignments,
                SessionExpirationDate = DateTime.Now.AddSeconds(_config.Value.sessionExpirationTime)
            };

            sessionDictionary.AddOrUpdate(GetSessionID(), sessionDictionaryToAddOrUpdate, (key, oldValue) => sessionDictionaryToAddOrUpdate);
        }

        /// <summary>
        /// Get session assignments
        /// </summary>
        /// <returns>assignments</returns>
        /// <returns>NULL when current session has no assignments</returns>
        private List<Assignment> GetSessionAssignments()
        {
            //Get assignment from current session
            string assignmentsStr = HttpContext.Session.GetString("assignments");

            //Init return object
            List<Assignment> assignments = null;

            //Add assignments to return object only if current session has assignments
            if (!string.IsNullOrWhiteSpace(assignmentsStr) && assignmentsStr.Length > 0)
            {
                assignments = assignmentsStr == null ? default : JsonConvert.DeserializeObject<List<Assignment>>(assignmentsStr);
            }

            //Add or update the session dictionary
            AddOrUpdateSessionDictionary(assignments);

            return assignments;
        }

        #endregion

        #region Directories And Files Handlers

        /// <summary>
        /// Save image to current session directory
        /// </summary>
        /// <param name="file"></param>
        private void SaveFile(IFormFile file)
        {
            #region Validations

            if(file == null) 
            {
                throw new ArgumentNullException("File must not be null");
            }

            #endregion

            try
            {
                //Path of the current session folder
                //Path pattern: rootDirectory/AssignmentImages/sessionid
                var dirPath = Path.Combine(Directory.GetCurrentDirectory(), "AssignmentImages", GetSessionID());

                //Create new directory if not exists
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                //Creating full path for the file.
                string filePath = Path.Combine(dirPath, file.FileName);

                //Saving the file in the session folder.
                using (Stream stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error saving file.", e);
            }
        }

        /// <summary>
        /// Delete Directory 
        /// </summary>
        /// <param name="key">Session ID - Represents the folder needs to delete</param>
        private void DeleteDirectory(string key)
        {
            #region Validations

            if (string.IsNullOrWhiteSpace(key)) 
            {
                throw new ArgumentException("Key must not be null or empty");
            }
            
            #endregion

            //Path of the session folder that needs to be deleted
            //Path pattern: rootDirectory/AssignmentImages/sessionid
            var dirPath = Path.Combine(Directory.GetCurrentDirectory(), "AssignmentImages", key);

            try
            {
                //Check if folder exists
                if (Directory.Exists(dirPath)) 
                {
                    //Delete the directory and its content
                    Directory.Delete(@dirPath, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Faild to delete directory and its content: {0}", e.Message);
            }
        }

        #endregion
    }
}
