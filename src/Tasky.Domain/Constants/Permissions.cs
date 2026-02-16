namespace Tasky.Domain.Constants;

public static class Permissions
{
    public static class Projects
    {
        public const string View = "projects.view";
        public const string Create = "projects.create";
        public const string Update = "projects.update";
        public const string Delete = "projects.delete";
    }

    public static class Boards
    {
        public const string View = "boards.view";
        public const string Manage = "boards.manage";
    }

    public static class Tasks
    {
        public const string View = "tasks.view";
        public const string Create = "tasks.create";
        public const string Update = "tasks.update";
        public const string Delete = "tasks.delete";
        public const string Assign = "tasks.assign";
        public const string ChangeStatus = "tasks.change_status";
    }

    public static class Comments
    {
        public const string View = "comments.view";
        public const string Create = "comments.create";
        public const string Delete = "comments.delete";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Manage = "users.manage";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Manage = "roles.manage";
    }

    public static IEnumerable<string> All()
    {
        yield return Projects.View;
        yield return Projects.Create;
        yield return Projects.Update;
        yield return Projects.Delete;
        
        yield return Boards.View;
        yield return Boards.Manage;
        
        yield return Tasks.View;
        yield return Tasks.Create;
        yield return Tasks.Update;
        yield return Tasks.Delete;
        yield return Tasks.Assign;
        yield return Tasks.ChangeStatus;
        
        yield return Comments.View;
        yield return Comments.Create;
        yield return Comments.Delete;
        
        yield return Users.View;
        yield return Users.Manage;
        
        yield return Roles.View;
        yield return Roles.Manage;
    }
}
