using Sandbox.Common.ObjectBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;

namespace $RootNamespace$.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MyProgrammableBlock), false, "MY_BlockSubType")]
    public class MyFirstBlock : MyGameLogicComponent
    {
    }
}