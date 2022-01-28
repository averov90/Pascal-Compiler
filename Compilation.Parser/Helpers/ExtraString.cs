using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilation.Parser {
    public class StringBox {
        char[] str;
        int[] numbers;
        int extraNumber;
        public StringBox(string str, int start_number = 0, bool progression = true) {
            this.str = str.ToCharArray();
            extraNumber = start_number;
            if (progression)
                numbers = Enumerable.Range(start_number, str.Length).ToArray();
            else {
                numbers = new int[str.Length];
                Array.Fill(numbers, start_number);
            }

        }

        StringBox(IEnumerable<char> str, IEnumerable<int> numbers, int extraNumber) {
            this.str = str.ToArray();
            this.numbers = numbers.ToArray();
            this.extraNumber = extraNumber;
        }

        public StringBox Remove(int start) {
            return new StringBox(str.Take(start), numbers.Take(start), numbers[0]);
        }

        /// <summary>
        /// RemoveRange(1,2) from "abcd" -> "ad"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox RemoveRange(int start, int end) {
            return new StringBox(str.Take(start).Concat(str.Skip(end + 1)), numbers.Take(start).Concat(numbers.Skip(end + 1)), numbers[0]);
        }

        /// <summary>
        /// RemoveSub(1,1) from "abcd" -> "acd"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox RemoveSub(int start, int count) {
            return new StringBox(str.Take(start).Concat(str.Skip(start + count)), numbers.Take(start).Concat(numbers.Skip(start + count)), numbers[0]);
        }

        public StringBox Substring(int start) {
            return new StringBox(str.Skip(start), numbers.Skip(start), numbers[0]);
        }

        public StringBox Substring(int start, int count) {
            return new StringBox(str.Skip(start).Take(count), numbers.Skip(start).Take(count), numbers[0]);
        }

        /// <summary>
        /// SubstringRange(1,2) from "abcd" -> "bc"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox SubstringRange(int start, int end) {
            return new StringBox(str.Skip(start).Take(end - start + 1), numbers.Skip(start).Take(end - start + 1), numbers[0]);
        }

        /// <summary>
        /// ReplaceRange(1,2,"?") from "abcd" -> "a?cd"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public StringBox ReplaceRange(int start, int end, StringBox newVal) {
            return new StringBox(str.Take(start).Concat(newVal.str).Concat(str.Skip(end)), numbers.Take(start).Concat(newVal.numbers).Concat(numbers.Skip(end)), numbers[0]);
        }

        public void Concatenate(StringBox another) {
            str = str.Concat(another.str).ToArray();
            numbers = numbers.Concat(another.numbers).ToArray();
        }

        public StringBox Plus(StringBox another) {
            return new StringBox(str.Concat(another.str), numbers.Concat(another.numbers), numbers[0]);
        }

        public StringBox[] Split(char delimiter) {
            if (str.Length == 0) return Array.Empty<StringBox>();

            List<StringBox> boxes = new();

            int prew_i = -1;
            for (int i = 0; i != str.Length; ++i) {
                if (str[i] == delimiter) {
                    if (prew_i != -1) {
                        boxes.Add(new StringBox(str.Skip(prew_i).Take(i - prew_i), numbers.Skip(prew_i).Take(i - prew_i), numbers[prew_i]));
                        prew_i = i + 1;
                    } else {
                        boxes.Add(new StringBox(str.Take(i), numbers.Take(i), numbers[0]));
                        prew_i = i + 1;
                    }
                }
            }

            if (prew_i != -1) {
                int num_payload = (str.Length != prew_i ? numbers[prew_i] : numbers[prew_i - 1]);
                boxes.Add(new StringBox(str.Skip(prew_i).Take(str.Length - prew_i), numbers.Skip(prew_i).Take(numbers.Length - prew_i), num_payload));
            } else {
                boxes.Add(new StringBox(str.Take(str.Length), numbers.Take(numbers.Length), numbers[0]));
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

        public StringBox[] Split(string delimiter) {
            if (str.Length == 0) return Array.Empty<StringBox>();
            int delen = delimiter.Length - 1;
            List<StringBox> boxes = new();

            int prew_i = -1, now_check = 0;
            for (int i = 0; i != str.Length; ++i) {
                if (str[i] == delimiter[now_check]) {
                    if (++now_check == delimiter.Length) {
                        now_check = 0;
                        if (prew_i != -1) {
                            boxes.Add(new StringBox(str.Skip(prew_i).Take(i - prew_i - delen), numbers.Skip(prew_i).Take(i - prew_i - delen), numbers[prew_i]));
                            prew_i = i + 1;
                        } else {
                            boxes.Add(new StringBox(str.Take(i - delen), numbers.Take(i - delen), numbers[0]));
                            prew_i = i + 1;
                        }
                    }
                } else {
                    i -= now_check;
                    now_check = 0;
                }
            }

            if (prew_i != -1) {
                int num_payload = (str.Length != prew_i ? numbers[prew_i] : numbers[prew_i - 1]);
                boxes.Add(new StringBox(str.Skip(prew_i).Take(str.Length - prew_i), numbers.Skip(prew_i).Take(numbers.Length - prew_i), num_payload));
            } else {
                boxes.Add(new StringBox(str.Take(str.Length), numbers.Take(numbers.Length), numbers[0]));
            }

            return boxes.ToArray();
        }

        public StringBox[] Split(string delimiter, Func<int, bool> verify_delimiter) {
            if (str.Length == 0) return Array.Empty<StringBox>();
            int delen = delimiter.Length - 1;
            List<StringBox> boxes = new();

            int prew_i = -1, now_check = 0;
            for (int i = 0; i != str.Length; ++i) {
                if (str[i] == delimiter[now_check]) {
                    if (++now_check == delimiter.Length) {
                        now_check = 0;
                        if (verify_delimiter(numbers[i])) {
                            if (prew_i != -1) {
                                boxes.Add(new StringBox(str.Skip(prew_i).Take(i - prew_i - delen), numbers.Skip(prew_i).Take(i - prew_i - delen), numbers[prew_i]));
                                prew_i = i + 1;
                            } else {
                                boxes.Add(new StringBox(str.Take(i - delen), numbers.Take(i - delen), numbers[0]));
                                prew_i = i + 1;
                            }
                        }
                    }
                } else {
                    i -= now_check;
                    now_check = 0;
                }
            }

            if (prew_i != -1) {
                int num_payload = (str.Length != prew_i ? numbers[prew_i] : numbers[prew_i - 1]);
                boxes.Add(new StringBox(str.Skip(prew_i).Take(str.Length - prew_i), numbers.Skip(prew_i).Take(numbers.Length - prew_i), num_payload));
            } else {
                boxes.Add(new StringBox(str.Take(str.Length), numbers.Take(numbers.Length), numbers[0]));
            }

            return boxes.ToArray();
        }

        public bool Contains(string substr) {
            return string.Join("", str).Contains(substr);
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
