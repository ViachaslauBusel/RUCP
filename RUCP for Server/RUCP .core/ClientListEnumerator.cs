﻿using RUCP.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class ClientListEnumerator : IEnumerator<ClientSocket>
    {
        private IEnumerator<KeyValuePair<long, ClientSocket>> keyValue;

        public ClientListEnumerator(IEnumerator<KeyValuePair<long, ClientSocket>> keyValue)
        {
            this.keyValue = keyValue;
        }
        public ClientSocket Current => keyValue.Current.Value;

        object IEnumerator.Current => keyValue.Current.Value;

        public void Dispose()
        {
            keyValue.Dispose();
        }

        public bool MoveNext()
        {
           return keyValue.MoveNext();
        }

        public void Reset()
        {
            keyValue.Reset();
        }
    }
}
