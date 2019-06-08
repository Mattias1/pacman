using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class PercentageArray<T>
    {
        PercentageElement<T>[] array;

        public PercentageElement<T> this[int index] {
            get { return this.array[index]; }
            set { this.array[index] = value; }
        }
        public int Length {
            get { return this.array.Length; }
        }

        public PercentageArray(int length) {
            this.array = new PercentageElement<T>[length];
        }

        /// <summary>
        /// Return the factor used to lerp between (values from) two indices
        /// </summary>
        /// <param name="toIndex"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public float GetLerpFactor(int toIndex, float percentage) {
            float from = this[toIndex - 1].Percentage;
            float to = this[toIndex].Percentage;
            return (percentage - from) / (to - from);
        }

        /// <summary>
        /// Return the index belonging to the first percentage value larger than the given percentage.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public int GetToIndex(float percentage) {
            int i = 0;
            while (++i < this.array.Length)
                if (percentage <= this.array[i].Percentage)
                    break;
            return i;
        }

        public PercentageArray<T> Clone() {
            PercentageArray<T> clone = new PercentageArray<T>(this.array.Length);
            for (int i = 0; i < clone.Length; i++)
                clone[i] = this.array[i].Clone();
            return clone;
        }

        /// <summary>
        /// A percentageArray with only one value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static PercentageArray<T> Singleton(T value) {
            PercentageArray<T> array = new PercentageArray<T>(1);
            array[0] = new PercentageElement<T>(0, value);
            return array;
        }
        /// <summary>
        /// A percentage array with certain values. The percentages are evenly spaced.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static PercentageArray<T> FromArray(T[] values) {
            PercentageArray<T> array = new PercentageArray<T>(values.Length);
            for (int i = 0; i < values.Length; i++)
                array[i] = new PercentageElement<T>(100 * i / (values.Length - 1), values[i]);
            return array;
        }
        /// <summary>
        /// A percentage array with percentages and values
        /// </summary>
        /// <param name="values">Params in the form: Pct, Value, Pct, Value, ...</param>
        /// <returns></returns>
        public static PercentageArray<T> FromValues(params object[] values) {
            return FromParams(values);
        }
        /// <summary>
        /// A percentage array with percentages and values
        /// </summary>
        /// <param name="values">Params in the form: Pct, Value, Pct, Value, ...</param>
        /// <returns></returns>
        public static PercentageArray<T> FromParams(object[] values) {
            PercentageArray<T> array = new PercentageArray<T>(values.Length / 2);
            for (int i = 0; i < values.Length; i += 2)
                array[i / 2] = new PercentageElement<T>((float)values[i], (T)values[i + 1]);
            return array;
        }
    }

    public class PercentageElement<T>
    {
        public float Percentage;
        public T Value;

        public PercentageElement(float percentage, T value) {
            this.Percentage = percentage;
            this.Value = value;
        }

        public PercentageElement<T> Clone() {
            return new PercentageElement<T>(this.Percentage, this.Value);
        }
    }
}
