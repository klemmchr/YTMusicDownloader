using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    internal interface IService<T> where T : Video
    {
        T GetVideo(string uri);
        IEnumerable<T> GetAllVideos(string uri);
    }
}
