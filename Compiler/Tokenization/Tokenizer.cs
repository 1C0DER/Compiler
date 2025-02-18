﻿using Compiler.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compiler.Tokenization
{
    /// <summary>
    /// A tokenizer for the reader language
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// The reader getting the characters from the file
        /// </summary>
        private IFileReader Reader { get; }

        /// <summary>
        /// The characters currently in the token
        /// </summary>
        private StringBuilder TokenSpelling { get; } = new StringBuilder();

        /// <summary>
        /// Createa a new tokenizer
        /// </summary>
        /// <param name="reader">The reader to get characters from the file</param>
        /// <param name="reporter">The error reporter to use</param>
        public Tokenizer(IFileReader reader, ErrorReporter reporter)
        {
            Reader = reader;
            Reporter = reporter;
        }

        public Tokenizer(StringReader stringReader, ErrorReporter errorReporter)
        {
        }

        /// <summary>
        /// Gets all the tokens from the file
        /// </summary>
        /// <returns>A list of all the tokens in the file in the order they appear</returns>
        public List<Token> GetAllTokens()
        {
            List<Token> tokens = new List<Token>();
            Token token = GetNextToken();
            while (token.Type != TokenType.EndOfText)
            {
                tokens.Add(token);
                token = GetNextToken();
            }
            tokens.Add(token);
            Reader.Close();
            return tokens;
        }

        /// <summary>
        /// Scan the next token
        /// </summary>
        /// <returns>True if and only if there is another token in the file</returns>
        private Token GetNextToken()
        {
            // Skip forward over any white spcae and comments
            SkipSeparators();

            // Remember the starting position of the token
            Position tokenStartPosition = Reader.CurrentPosition;

            // Scan the token and work out its type
            TokenType tokenType = ScanToken();

            // Create the token
            Token token = new Token(tokenType, TokenSpelling.ToString(), tokenStartPosition);
            Debugger.Write($"Scanned {token}");

            // Report an error if necessary
            if (tokenType == TokenType.Error)
            {
                // Report the error here
            }

            return token;
        }

        /// <summary>
        /// Skip forward until the next character is not whitespace or a comment
        /// </summary>
        private void SkipSeparators()
        {
            while (Reader.Current == '!' || IsWhiteSpace(Reader.Current))
            {
                if (Reader.Current == '!')
                {
                    Reader.SkipRestOfLine();
                }
                else
                    Reader.MoveNext();
            }
        }

        /// <summary>
        /// Find the next token
        /// </summary>
        /// <returns>The type of the next token</returns>
        /// <remarks>Sets tokenSpelling to be the characters in the token</remarks>
        private TokenType ScanToken()
        {
            TokenSpelling.Clear();

            // Check for Identifier: [$]letter(letter|digit)*[$]
            if (Reader.Current == '$' || char.IsLetter(Reader.Current))
            {
                if (Reader.Current == '$')
                    TakeIt(); // Optional starting $

                // Read the identifier part
                if (!char.IsLetter(Reader.Current))
                    return TokenType.Error; // Identifier must start with a letter

                TakeIt();
                while (char.IsLetterOrDigit(Reader.Current))
                    TakeIt();

                if (Reader.Current == '$')
                    TakeIt(); // Optional ending $

                return TokenType.Identifier;
            }

            // Check for Int-Literal: digit(,|digit)*
            else if (char.IsDigit(Reader.Current))
            {
                TakeIt();
                while (char.IsDigit(Reader.Current) || Reader.Current == ',')
                {
                    if (Reader.Current == ',' && !char.IsDigit(PeekNextChar()))
                        return TokenType.Error; // Commas must be followed by digits
                    TakeIt();
                }
                return TokenType.IntLiteral;
            }

            // Check for Char-Literal: 'character'
            else if (Reader.Current == '\'')
            {
                TakeIt(); // Opening '
                if (char.IsLetterOrDigit(Reader.Current) || IsGraphicSymbol(Reader.Current))
                    TakeIt();
                else
                    return TokenType.Error; // Invalid character inside the literal

                if (Reader.Current == '\'')
                {
                    TakeIt(); // Closing '
                    return TokenType.CharLiteral;
                }
                else
                {
                    return TokenType.Error; // Missing closing '
                }
            }

            // Check for Operators: +, -, *, /, <, >, =, \, #
            else if (IsOperator(Reader.Current))
            {
                TakeIt();
                return TokenType.Operator;
            }

            // Check for Reserved Symbols: :, :=, ;, ~, (, )
            else if (Reader.Current == ':')
            {
                TakeIt();
                if (Reader.Current == '=')
                {
                    TakeIt();
                    return TokenType.Becomes;
                }
                return TokenType.Colon;
            }
            else if (Reader.Current == ';')
            {
                TakeIt();
                return TokenType.Semicolon;
            }
            else if (Reader.Current == '~')
            {
                TakeIt();
                return TokenType.Is;
            }
            else if (Reader.Current == '(')
            {
                TakeIt();
                return TokenType.LeftBracket;
            }
            else if (Reader.Current == ')')
            {
                TakeIt();
                return TokenType.RightBracket;
            }

            // Handle End of Text
            else if (Reader.Current == default(char))
            {
                return TokenType.EndOfText;
            }

            // Unknown or invalid token
            else
            {
                TakeIt();
                return TokenType.Error;
            }
        }

        private static bool IsGraphicSymbol(char c)
        {
            return c == '.' || c == ',' || c == '?' || c == '!' || c == ' ';
        }

        private char PeekNextChar()
        {
            return Reader.PeekNext();
        }

        /// <summary>
        /// Appends the current character to the current token then moves to the next character
        /// </summary>
        private void TakeIt()
        {
            TokenSpelling.Append(Reader.Current);
            Reader.MoveNext();
        }

        /// <summary>
        /// Checks whether a character is white space
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if and only if c is a whitespace character</returns>
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }

        /// <summary>
        /// Checks whether a character is an operator
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if and only if the character is an operator in the language</returns>
        private static bool IsOperator(char c)
        {
            switch (c)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '<':
                case '>':
                case '=':
                case '\\':
                    return true;
                default:
                    return false;
            }
        }
    }
}
