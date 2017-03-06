namespace CVaS.Shared.Services.Argument
{
    public abstract class Argument
    {
        public ArgumentType Type { get; set; }

        protected Argument(ArgumentType type)
        {
            Type = type;
        }
    }

    public class FileArgument : Argument
    {
        public int FileId { get; set; }
        public string LocalPath { get; set; }
        public FileArgument(int fileId) : base(ArgumentType.File)
        {
            FileId = fileId;
        }

        public override string ToString()
        {
            return LocalPath;
        }
    }

    public class StringArgument : Argument
    {
        public string Content { get; set; }

        public StringArgument(string content) : base(ArgumentType.String)
        {
            Content = content;
        }

        public override string ToString()
        {
            return "\"" + Content + "\"";
        }
    }

    public class GenericArgument<T> : Argument
    {
        public T Content { get; set; }

        public GenericArgument(T content) : base(ArgumentType.Raw)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content.ToString();
        }
    }

    public enum ArgumentType
    {
        String,
        File,
        Raw
    }
}