using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CVaS.Shared.Helpers
{
    public static class TaskExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SupressError(this Task task)
        {

        }
    }
}