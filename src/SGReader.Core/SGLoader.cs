﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGReader.Core
{
    public class SGLoader
    {
        public SGFile Load(string filePath)
        {
            return new SGFile(filePath);
        }
    }
}