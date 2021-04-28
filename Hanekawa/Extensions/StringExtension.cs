using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hanekawa.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex FilterRegex =
            new Regex(@"(?:discord(?:\.gg|.me|app\.com\/invite)\/(([\w]{16}|(?:[\w]+-?){3})))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static bool IsPictureUrl(this string str)
        {
            var isGif = str.EndsWith(".gif");
            var isPng = str.EndsWith(".png");
            var isJpeg = str.EndsWith(".jpeg");
            var isJpg = str.EndsWith(".jpg");

            if (isGif) return true;
            if (isPng) return true; 
            return isJpeg || isJpg;
        }

        public static bool IsDiscordInvite(this string str, out string invite)
        {
            if (FilterRegex.IsMatch(str))
            {
                var invites = FilterRegex.GetGroupNames().FirstOrDefault();
                invite = invites;
                return true;
            }

            invite = null;
            return false;
        }

        public static string ListToString(this IEnumerable<string> list) => string.Join("\n", list);

        public static bool FuzzyMatch(this string stringToSearch, string pattern, out int outScore)
        {
            // Score consts
            const int adjacencyBonus = 5; // bonus for adjacent matches
            const int separatorBonus = 10; // bonus if match occurs after a separator
            const int camelBonus = 10; // bonus if match is uppercase and prev is lower

            const int
                leadingLetterPenalty = -3; // penalty applied for every letter in stringToSearch before the first match
            const int maxLeadingLetterPenalty = -9; // maximum penalty for leading letters
            const int unmatchedLetterPenalty = -1; // penalty for every letter that doesn't matter


            // Loop variables
            var score = 0;
            var patternIdx = 0;
            var patternLength = pattern.Length;
            var strIdx = 0;
            var strLength = stringToSearch.Length;
            var prevMatched = false;
            var prevLower = false;
            var prevSeparator = true; // true if first letter match gets separator bonus

            // Use "best" matched letter if multiple string letters match the pattern
            char? bestLetter = null;
            char? bestLower = null;
            var bestLetterScore = 0;

            // Loop over strings
            while (strIdx != strLength)
            {
                var patternChar = patternIdx != patternLength ? pattern[patternIdx] as char? : null;
                var strChar = stringToSearch[strIdx];

                var patternLower = patternChar != null ? char.ToLower((char) patternChar) as char? : null;
                var strLower = char.ToLower(strChar);
                var strUpper = char.ToUpper(strChar);

                var nextMatch = patternChar != null && patternLower == strLower;
                var rematch = bestLetter != null && bestLower == strLower;

                var advanced = nextMatch && bestLetter != null;
                var patternRepeat = bestLetter != null && patternChar != null && bestLower == patternLower;
                if (advanced || patternRepeat)
                {
                    score += bestLetterScore;
                    bestLetter = null;
                    bestLower = null;
                    bestLetterScore = 0;
                }

                if (nextMatch || rematch)
                {
                    var newScore = 0;

                    // Apply penalty for each letter before the first pattern match
                    // Note: Math.Max because penalties are negative values. So max is smallest penalty.
                    if (patternIdx == 0)
                    {
                        var penalty = Math.Max(strIdx * leadingLetterPenalty, maxLeadingLetterPenalty);
                        score += penalty;
                    }

                    // Apply bonus for consecutive bonuses
                    if (prevMatched)
                        newScore += adjacencyBonus;

                    // Apply bonus for matches after a separator
                    if (prevSeparator)
                        newScore += separatorBonus;

                    // Apply bonus across camel case boundaries. Includes "clever" isLetter check.
                    if (prevLower && strChar == strUpper && strLower != strUpper)
                        newScore += camelBonus;

                    // Update pattern index IF the next pattern letter was matched
                    if (nextMatch)
                        ++patternIdx;

                    // Update best letter in stringToSearch which may be for a "next" letter or a "rematch"
                    if (newScore >= bestLetterScore)
                    {
                        // Apply penalty for now skipped letter
                        if (bestLetter != null)
                            score += unmatchedLetterPenalty;

                        bestLetter = strChar;
                        bestLower = char.ToLower((char) bestLetter);
                        bestLetterScore = newScore;
                    }

                    prevMatched = true;
                }
                else
                {
                    score += unmatchedLetterPenalty;
                    prevMatched = false;
                }

                // Includes "clever" isLetter check.
                prevLower = strChar == strLower && strLower != strUpper;
                prevSeparator = strChar is '_' or ' ';

                ++strIdx;
            }

            // Apply score for last match
            if (bestLetter != null) score += bestLetterScore;

            outScore = score;
            return patternIdx == patternLength;
        }
    }
}