using System;

namespace UsingCastlesWcfFacility
{
    public class Operations : IOperations
    {
        private int _value;

        /// <summary>
        /// Note the injected value.
        /// This can't be done with WCF's standard
        /// factory because it doesn't support dependency injection.
        /// </summary>
        /// <param name="value"></param>
        public Operations(int value)
        {
            _value = value;
        }


        public int GetValueFromConstructor()
        {
            return _value;
        }
    }
}