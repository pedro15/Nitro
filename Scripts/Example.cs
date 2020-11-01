using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Nitro.Scripts
{
    public abstract class Example
    {
        public Example(float n1)
        {

        }
    }

    public abstract class ExampleChildren : Example
    {
        public ExampleChildren(float x) : base(x)
        {

        }
    }
}
