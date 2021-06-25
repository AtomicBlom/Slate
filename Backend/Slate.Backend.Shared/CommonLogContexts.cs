﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;

namespace Slate.Backend.Shared
{
    public static class CommonLogContexts
    {
        public static IDisposable ApplicationInstanceId(Guid id) => LogContext.PushProperty("ApplicationInstanceId", id);
    }
}
