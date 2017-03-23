namespace CVaS.Shared.Services.Argument
{
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
}