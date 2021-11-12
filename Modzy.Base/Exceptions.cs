using System;
using System.Collections.Generic;
using System.Text;

namespace Modzy;
public class RuntimeNotInitializedException : Exception
{
    public RuntimeNotInitializedException(Runtime o) : base($"This runtime object is not initialized.") {}
}


