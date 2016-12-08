using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CVaS.BL.Helpers
{
    public static class TaskExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SupressError(this Task task)
        {

        }
    }
}