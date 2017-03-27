using System;

namespace CVaS.Shared.Services.Argument
{
    public class FileArgument : Argument
    {
        public Guid FileId { get; set; }
        public string LocalPath { get; set; }
        public FileArgument(Guid fileId) : base(ArgumentType.File)
        {
            FileId = fileId;
        }

        public override string ToString()
        {
            return LocalPath;
        }
    }
}