using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilation.Lexer {
    public class StringBox {
        char[] str;
        int[] numbers;
        int extraNumber = -1;
        public StringBox(string str, int start_number = 0) {
            this.str = str.ToCharArray();
            numbers = Enumerable.Range(start_number, str.Length).ToArray();
        }

        StringBox(string str, int start_number, int extraNumber) {
            this.str = str.ToCharArray();
            numbers = Enumerable.Range(start_number, str.Length).ToArray();
            this.extraNumber = extraNumber;
        }

        public StringBox Remove(int start) {
            extraNumber = numbers[0];
            str = str.Take(start).ToArray();
            numbers = numbers.Take(start).ToArray();
            return this;
        }

        /// <summary>
        /// RemoveRange(1,2) from "abcd" -> "ad"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox RemoveRange(int start, int end) {
            extraNumber = numbers[0];
            str = str.Take(start).Concat(str.Skip(end + 1)).ToArray();
            numbers = numbers.Take(start).Concat(numbers.Skip(end + 1)).ToArray();
            return this;
        }

        /// <summary>
        /// RemoveSub(1,1) from "abcd" -> "acd"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox RemoveSub(int start, int count) {
            extraNumber = numbers[0];
            str = str.Take(start).Concat(str.Skip(start + count)).ToArray();
            numbers = numbers.Take(start).Concat(numbers.Skip(start + count)).ToArray();
            return this;
        }

        public StringBox Substring(int start) {
            extraNumber = numbers[0];
            str = str.Skip(start).ToArray();
            numbers = numbers.Skip(start).ToArray();
            return this;
        }

        public StringBox Substring(int start, int count) {
            extraNumber = numbers[0];
            str = str.Skip(start).Take(count).ToArray();
            numbers = numbers.Skip(start).Take(count).ToArray();
            return this;
        }

        /// <summary>
        /// SubstringRange(1,2) from "abcd" -> "bc"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox SubstringRange(int start, int end) {
            extraNumber = numbers[0];
            str = str.Skip(start).Take(end - start + 1).ToArray();
            numbers = numbers.Skip(start).Take(end - start + 1).ToArray();
            return this;
        }

        public StringBox[] Split(char delimiter) {
            if (str.Length == 0) return Array.Empty<StringBox>();

            List<StringBox> boxes = new();

            int prew_i = -1;
            for (int i = 0; i != str.Length; ++i) {
                if (str[i] == delimiter) {
                    if (prew_i != -1) {
                        boxes.Add(new StringBox(string.Join("", str.Skip(prew_i).Take(i - prew_i)), numbers[prew_i], numbers[prew_i]));
                        prew_i = i + 1;
                    } else {
                        boxes.Add(new StringBox(string.Join("", str.Take(i)), numbers[0], numbers[0]));
                        prew_i = i + 1;
                    }
                }
            }

            if (prew_i != -1) {
                int num_payload = (str.Length != prew_i ? numbers[prew_i] : numbers[prew_i - 1]);
                boxes.Add(new StringBox(string.Join("", str.Skip(prew_i).Take(str.Length - prew_i)), num_payload, num_payload));
            } else {
                boxes.Add(new StringBox(string.Join("", str.Take(str.Length)), numbers[0], numbers[0]));
            }

            return boxes.ToArray();
        }
        public bool Contains(char ch) {
            foreach (var item in str) {
                if (item == ch)
                    return true;
            }
            return false;
        }

        public char this[int i] {
            get { return str[i]; }
        }

        public int Length { get { return str.Length; } }

        public int StartNumber { get { return (numbers.Length != 0 ? numbers[0] : extraNumber); } }

        public string ExtactString() {
            return string.Join("", str);
        }

        public int[] ExtactNumbers() {
            return numbers;
        }

        public char CharAT(int i) {
            return str[i];
        }

        public int NumberAT(int i) {
            return numbers[i];
        }
    }
}
