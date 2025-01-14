﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

#nullable enable
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageGlass.Base {

    public partial class Helpers {
        private static JsonSerializerOptions JsonOptions { get; } = new() {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            Converters = {
                // Write enum value as string
                new JsonStringEnumConverter(),

                new CustomDateTimeConverter(Constants.DATETIME_FORMAT),
            },

            // ignoring policy
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
        };


        /// <summary>
        /// Parse JSON string to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T? ParseJson<T>(string json) {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }


        /// <summary>
        /// Parse JSON from a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<T?> ParseJson<T>(Stream stream) {
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }


        /// <summary>
        /// Reads JSON file and parses to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonFilePath"></param>
        /// <returns></returns>
        public static T? ReadJson<T>(string jsonFilePath) {
            using var stream = File.OpenRead(jsonFilePath);

            return JsonSerializer.Deserialize<T>(stream, JsonOptions);
        }


        /// <summary>
        /// Writes an object value to JSON file
        /// </summary>
        /// <param name="jsonFilePath"></param>
        /// <param name="value"></param>
        public static void WriteJson(string jsonFilePath, object? value) {
            var json = JsonSerializer.Serialize(value, JsonOptions);

            File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
        }
    }


    public class CustomDateTimeConverter: JsonConverter<DateTime> {
        private readonly string Format;
        public CustomDateTimeConverter(string format) {
            Format = format;
        }

        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options) {
            writer.WriteStringValue(date.ToString(Format));
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return DateTime.ParseExact(reader.GetString(), Format, null);
        }
    }
}
