    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace SmartTask.Domain.Constants
    {
        public static class Permissions
        {
            public static class Task
            {
                public const string Create = "Task.Create";
                public const string View = "Task.View";
                public const string Edit = "Task.Edit";
                public const string Delete = "Task.Delete";
                public const string Assign = "Task.Assign";
                public const string Archive = "Task.Archive";
                public const string Comment = "Task.Comment";
                public const string Upload = "Task.Upload";
            }
            public static class Project
            {
                public const string Create = "Project.Create";
                public const string View = "Project.View";
                public const string Edit = "Project.Edit";
                public const string Delete = "Project.Delete";
                public const string AssignLead = "Project.AssignLead";
                public const string Archive = "Project.Archive";
                public const string ManageMembers = "Project.ManageMembers";
                public const string UploadFiles = "Project.UploadFiles";
            }


            public static class User
            {
                public const string Create = "User.Create";
                public const string View = "User.View";
                public const string Edit = "User.Edit";
                public const string Delete = "User.Delete";
                public const string Activate = "User.Activate";
                public const string Deactivate = "User.Deactivate";
                public const string AssignRole = "User.AssignRole";
            }

            public static class Role
            {
                public const string Create = "Role.Create";
                public const string View = "Role.View";
                public const string Edit = "Role.Edit";
                public const string Delete = "Role.Delete";
                public const string AssignPermissions = "Role.AssignPermissions";
            }

            public static class Company
            {
                public const string Create = "Company.Create";
                public const string View = "Company.View";
                public const string Edit = "Company.Edit";
                public const string Delete = "Company.Delete";
                public const string AssignUsers = "Company.AssignUsers";
            }

            public static class Report
            {
                public const string Generate = "Report.Generate";
                public const string View = "Report.View";
                public const string Export = "Report.Export";
            }

            public static class Audit
            {
                public const string View = "Audit.View";
                public const string Download = "Audit.Download";
            }

            public static class Settings
            {
                public const string View = "Settings.View";
                public const string Update = "Settings.Update";
            }

            public static List<string> All
            {
                get
                {
                    return typeof(Permissions)
                        .GetNestedTypes()
                        .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                        .Select(f => f.GetValue(null)?.ToString())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                }
            }
        }


    }
