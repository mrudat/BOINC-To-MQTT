// <copyright file="TheoryDataStuff.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT.Tests.Scaffolding;

public static class TheoryDataStuff
{
    /// <summary>
    /// Returns a <see cref="TheoryData{string}"/> with a list of names of classes from <paramref name="classes"/> that implement <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">An interface that may be implemented by the classes in <paramref name="classes"/>.</typeparam>
    /// <param name="classes">A list of classes that may implement <typeparamref name="TInterface"/>.</param>
    /// <returns>A <see cref="TheoryData{string}"/> with a list of names of classes from <paramref name="classes"/> that implement <typeparamref name="TInterface"/>.</returns>
    public static TheoryData<string> Implementing<TInterface>(IEnumerable<Type> classes) => new(classes
        .Where(clazz => clazz.IsAssignableTo(typeof(TInterface)))
        .Select(clazz => clazz.Name));
}
