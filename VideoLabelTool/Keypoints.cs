using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLabelTool
{
    public class Keypoint
    {
        public float x;
        public float y;
        public float visibility;

        public Keypoint(float X, float Y, float Visibility)
        {
            this.x = X;
            this.y = Y;
            this.visibility = Visibility;
        }
    }

    public class Keypoints : IEnumerable
    {
        public Keypoint[] pose;
        public Keypoints(float x1, float y1, float v1, 
                         float x2, float y2, float v2,
                         float x3, float y3, float v3,
                         float x4, float y4, float v4,
                         float x5, float y5, float v5,
                         float x6, float y6, float v6,
                         float x7, float y7, float v7,
                         float x8, float y8, float v8,
                         float x9, float y9, float v9,
                         float x10, float y10, float v10,
                         float x11, float y11, float v11,
                         float x12, float y12, float v12,
                         float x13, float y13, float v13,
                         float x14, float y14, float v14,
                         float x15, float y15, float v15,
                         float x16, float y16, float v16,
                         float x17, float y17, float v17
                        )
        {
            pose = new Keypoint[17]
            {
                new Keypoint(x1, y1, v1),
                new Keypoint(x2, y2, v2),
                new Keypoint(x3, y3, v3),
                new Keypoint(x4, y4, v4),
                new Keypoint(x5, y5, v5),
                new Keypoint(x6, y6, v6),
                new Keypoint(x7, y7, v7),
                new Keypoint(x8, y8, v8),
                new Keypoint(x9, y9, v9),
                new Keypoint(x10, y10, v10),
                new Keypoint(x11, y11, v11),
                new Keypoint(x12, y12, v12),
                new Keypoint(x13, y13, v13),
                new Keypoint(x14, y14, v14),
                new Keypoint(x15, y15, v15),
                new Keypoint(x16, y16, v16),
                new Keypoint(x17, y17, v17)
            };
        }

        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(pose);
        }

        private class MyEnumerator : IEnumerator
        {
            public Keypoint[] pose;
            int position = 1;

            public MyEnumerator(Keypoint[] list)
            {
                pose = list;
            }
            public IEnumerator GetEnumerator()
            {
                return (IEnumerator)this;
            }

            public bool MoveNext()
            {
                position++;
                return (position < pose.Length);
            }

            public void Reset()
            {
                position = 0;
            }

            public object Current
            {
                get
                {
                    try
                    {
                        return pose[position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
            }
        }
    }
}
