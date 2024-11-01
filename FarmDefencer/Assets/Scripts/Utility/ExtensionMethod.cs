using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Linq;

namespace JoonyleGameDevKit
{
    public static class ExtensionMethod
    {
        // Vector
        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static Vector2 ToVector2XZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        public static Vector3 ToVector3(this Vector2 vec, float z = 0)
        {
            return new Vector3(vec.x, vec.y, z);
        }
        public static Vector3 ToVector3XZ(this Vector2 vec, float y = 0)
        {
            return new Vector3(vec.x, y, vec.y);
        }
        public static Vector3 CombineWith(this Vector2 v2, Vector3 v3)
        {
            return new Vector3(v2.x + v3.x, v2.y + v3.y, v3.z);
        }
        public static Vector3 CombineWith(this Vector3 v3, Vector2 v2)
        {
            return new Vector3(v3.x + v2.x, v3.y + v2.y, v3.z);
        }

        // Random
        public static int RangeExcept(this System.Random random, int minInclusive, int maxExclusive, int except, int limitCount = 10)
        {
            if (minInclusive < 0 || maxExclusive < 0 || minInclusive >= maxExclusive)
            {
                Debug.LogError($"Invalid minInclusive or maxExclusive\n{StackTraceUtility.ExtractStackTrace()}");
                return except;
            }

            var currentCount = 0;
            var result = except;

            while (result == except)
            {
                if (currentCount >= limitCount)
                {
                    result = random.Next(minInclusive, maxExclusive);
                    break;
                }

                currentCount++;

                result = random.Next(minInclusive, maxExclusive);
            }

            return result;
        }

        // LayerMask
        public static int GetLayerNumber(this int layerMaskValue)
        {
            if (layerMaskValue == 0)
                return -1;

            int layerNumber = 0;

            while (layerMaskValue > 1)      // 1�� �Ǹ� ����
            {
                layerMaskValue = layerMaskValue >> 1;
                layerNumber++;
            }

            return layerNumber;
        }
        public static int GetLayerValue(this int layerMaskNumber)
        {
            if (layerMaskNumber is < 0 or > 31)
                return -1;

            return 1 << layerMaskNumber;
        }

        // String
        /// <summary>
        /// ���ڿ����� ���ڸ� ã�� �����Ѵ�
        /// </summary>
        public static int ExtractNumber(this string str, int defaultValue = 0)
        {
            // ���ڿ� �ڿ������� Ž���� �����Ѵ�
            int stringEndIndex = str.Length - 1;
            int numberStartIndex = -1;

            for (int i = stringEndIndex; i >= 0; i--)
            {
                // ���ڸ� ã�� ���
                if (char.IsDigit(str[i]))
                {
                    // ó���� ��� �ε����� ����Ѵ�
                    if (numberStartIndex == -1)
                    {
                        numberStartIndex = i;
                    }
                }
                // ���ڸ� ã�� ���
                else
                {
                    // �̹� ���ڸ� ��������� �ִٸ�
                    if (numberStartIndex != -1)
                    {
                        // ���� ������ ��ȯ�Ѵ�
                        return int.Parse(str.Substring(i + 1, numberStartIndex - i));
                    }
                }
            }

            // ����: ���ڿ� ��ü�� �����̰ų� ���� �տ� ������ �������� �ִ� ���
            if (numberStartIndex != -1)
            {
                return int.Parse(str.Substring(0, numberStartIndex + 1));
            }

            // ���ڸ� ã�� ���߰ų� ��ȯ�� ������ ��� ���ܸ� ������
            // throw new ArgumentException("No valid number found in the input string.");
            Debug.LogWarning("No valid number found in the input string.\nThen return defaultValue");
            return defaultValue;
        }
    }
}