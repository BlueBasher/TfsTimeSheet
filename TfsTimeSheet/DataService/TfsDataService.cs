﻿namespace TfsTimeSheet.DataService
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using TfsTimeSheet.Models;

    public class TfsDataService
    {
        #region Properties
        private TfsTeamProjectCollection _tfs;
        private WorkItemStore _workItemStore;
        private ICommonStructureService4 _commonStructureService;

        private string _project;
        private string _workitemQueryText;
        private string _activeState;
        private string _closedState;

        public bool IsAuthenticated
        {
            get
            {
                return _tfs != null &&
                       _tfs.ConfigurationServer != null &&
                       _tfs.ConfigurationServer.AuthorizedIdentity != null;
            }
        }
        public string UserName
        {
            get
            {
                return IsAuthenticated
                        ? _tfs.ConfigurationServer.AuthorizedIdentity.DisplayName
                        : "Not logged in";
            }
        }
        public int UserId
        {
            get
            {
                return IsAuthenticated
                        ? _tfs.ConfigurationServer.AuthorizedIdentity.UniqueUserId
                        : 0;
            }
        }
        #endregion

        #region Public Methods
        public void Connect(string url, string project, string workItemQuery, string activeState, string closedState)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException("project");
            }
            if (string.IsNullOrWhiteSpace(workItemQuery))
            {
                throw new ArgumentNullException("workItemQuery");
            }
            if (string.IsNullOrWhiteSpace(activeState))
            {
                throw new ArgumentNullException("activeState");
            }
            if (string.IsNullOrWhiteSpace(closedState))
            {
                throw new ArgumentNullException("closedState");
            }

            _project = project;
            _activeState = activeState;
            _closedState = closedState;

            _tfs = new TfsTeamProjectCollection(new Uri(url));
            _tfs.EnsureAuthenticated();

            _workItemStore = _tfs.GetService<WorkItemStore>();
            _commonStructureService = _tfs.GetService<ICommonStructureService4>();

            _workitemQueryText = GetWorkitemQueryText(workItemQuery);
        }

        public void Disconnect()
        {
            if (IsAuthenticated)
            {
                _tfs.Disconnect();
            }
        }

        public IEnumerable<TimeSheetItem> GetWorkItems(DateTime iterationDay, string userName)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("User not authenticated.");
            }

            var result = new List<TimeSheetItem>();

            var parameters = new Hashtable { { "project", _project }, { "me", userName } };
            var wiCollection = _workItemStore.Query(_workitemQueryText, parameters);

            // Add only workitems from an iteration that has the given iterationDay 
            result.AddRange(from WorkItem workItem in wiCollection
                            let fullPath = workItem.IterationPath.Replace(_project, _project + @"\Iteration")
                            let node = _commonStructureService.GetNodeFromPath(fullPath)
                            where node.StartDate <= iterationDay && node.FinishDate >= iterationDay
                            select new TimeSheetItem
                                {
                                    WorkItemId = workItem.Id,
                                    Name = workItem.Title
                                });

            return result;
        }

        public void UpdateWorkItemEffort(int id, TimeSpan delta)
        {
            var workItem = _workItemStore.GetWorkItem(id);
            var remaining = Convert.ToDouble(workItem["Remaining work"]);
            var completed = Convert.ToDouble(workItem["Completed work"]);

            if (remaining < delta.TotalHours)
            {
                throw new InvalidOperationException(string.Format("There are only {0} hours remaining. Can't book {1} hours.", remaining, delta.TotalHours));
            }

            workItem["Remaining work"] = remaining - delta.TotalHours;
            workItem["Completed work"] = completed + delta.TotalHours;
            workItem.State = _activeState;
            workItem.Save();
        }

        public void UpdateWorkItemClosedState(int id)
        {
            var workItem = _workItemStore.GetWorkItem(id);

            workItem["Remaining work"] = 0;
            workItem.State = _closedState;
            workItem.Save();
        }
        #endregion

        #region Private Methods
        private string GetWorkitemQueryText(string workItemQuery)
        {
            var workItemQueryPath = workItemQuery.Split('/');
            var project = _workItemStore.Projects[_project];
            var queryHierarchy = project.QueryHierarchy;
            var queryFolder = queryHierarchy as QueryFolder;
            QueryDefinition queryDefinition = null;

            for (var i = 0; i < workItemQueryPath.Count(); i++)
            {
                if (queryFolder == null)
                {
                    throw new InvalidOperationException(string.Format("Invalid queryPath supplied. {0}", workItemQuery));
                }

                var queryItem = queryFolder[workItemQueryPath[i]];

                if (i < workItemQueryPath.Count() - 1)
                {
                    queryFolder = queryItem as QueryFolder;

                }
                else
                {
                    queryDefinition = queryItem as QueryDefinition;
                }
            }

            if (queryDefinition == null)
            {
                throw new InvalidOperationException(string.Format("Invalid queryPath supplied. {0}", workItemQuery));
            }
            return queryDefinition.QueryText;
        }
        #endregion
    }
}