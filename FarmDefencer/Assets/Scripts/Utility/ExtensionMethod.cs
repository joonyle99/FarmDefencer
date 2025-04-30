using UnityEngine;

namespace JoonyleGameDevKit
{
    public static class ExtensionMethod
    {
        // Vector2
        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static Vector2 ToVector2XZ(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        // Vector3
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

        // Vector2Int
        public static Vector2 ToVector2(this Vector3Int vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static Vector2Int ToVector2Int(this Vector3Int vec)
        {
            return new Vector2Int(vec.x, vec.y);
        }
        public static Vector2Int ToVector2Int(this Vector2 vec)
        {
            return new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));
        }
        public static Vector2Int ToVector2Int(this Vector3 vec)
        {
            return new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));
        }

        // Vector3Int
        public static Vector3 ToVector3(this Vector2Int vec, float z = 0)
        {
            return new Vector3(vec.x, vec.y, z);
        }
        public static Vector2 ToVector2(this Vector2Int vec)
        {
            return new Vector2(vec.x, vec.y);
        }
        public static Vector3Int ToVector3Int(this Vector2Int vec, int z = 0)
        {
            return new Vector3Int(vec.x, vec.y, z);
        }
        public static Vector3 ToVector3(this Vector3Int vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }
        public static Vector3Int ToVector3Int(this Vector3 vec)
        {
            return new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));
        }
        public static Vector3Int ToVector3Int(this Vector2 vec)
        {
            return new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), 0);
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

            while (layerMaskValue > 1)      // 1이 되면 종료
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
        /// 문자열에서 숫자를 찾아 추출한다
        /// </summary>
        public static int ExtractNumber(this string str, int defaultValue = 0)
        {
            // 문자열 뒤에서부터 탐색을 시작한다
            int stringEndIndex = str.Length - 1;
            int numberStartIndex = -1;

            for (int i = stringEndIndex; i >= 0; i--)
            {
                // 숫자를 찾은 경우
                if (char.IsDigit(str[i]))
                {
                    // 처음인 경우 인덱스를 기록한다
                    if (numberStartIndex == -1)
                    {
                        numberStartIndex = i;
                    }
                }
                // 문자를 찾은 경우
                else
                {
                    // 이미 숫자를 기록한적이 있다면
                    if (numberStartIndex != -1)
                    {
                        // 숫자 구간을 반환한다
                        return int.Parse(str.Substring(i + 1, numberStartIndex - i));
                    }
                }
            }

            // 예외: 문자열 전체가 숫자이거나 가장 앞에 숫자의 시작점이 있는 경우
            if (numberStartIndex != -1)
            {
                return int.Parse(str.Substring(0, numberStartIndex + 1));
            }

            // 숫자를 찾지 못했거나 변환에 실패한 경우 예외를 던진다
            // throw new ArgumentException("No valid number found in the input string.");
            Debug.LogWarning("No valid number found in the input string.\nThen return defaultValue");
            return defaultValue;
        }

        // GameState
        public static string ToString(this GameState gameState)
        {
            return gameState switch
            {
                GameState.Normal => "Normal",
                GameState.Pause => "Pause",
                GameState.DefenceEnd => "DefenceEnd",
                GameState.Water => "Water",
                GameState.Build => "Build",
                GameState.Wave => "Wave",
                _ => "Unknown"
            };
        }
    }
}