using System.Collections;
using System.Threading.Tasks;

namespace Stereo.Utilities.Extensions;

public static class TaskExtensions
{
    public static IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }
}
