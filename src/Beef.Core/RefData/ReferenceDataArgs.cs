// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.RefData
{
    public class ReferenceDataArgs : EntityBase
    {
        private string _code;
        private string _text;

        /// <summary>
        /// Gets or set the 
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetValue(ref _code, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(Code));
        }

        public string Text
        {
            get => _text;
            set => SetValue(ref _text, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(Text));
        }

        public override bool IsInitial => throw new NotImplementedException();

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
