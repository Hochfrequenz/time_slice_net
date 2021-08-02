﻿using System;
using ExampleClasses.Music;
using NUnit.Framework;
using TimeSlice;

namespace TimeSliceTests
{
    /// <summary>
    ///     Test cases that are not thought to cover every edge case but rather demonstrate the main ideas of this library.
    ///     In this test class we're dealing with <see cref="TimeDependentParentChildRelationship{TParent,TChild}" />s.
    /// </summary>
    public class MusicianListenerRelationshipExampleTests
    {
        /// <summary>
        ///     demonstrates the use of the <see cref="TimeDependentParentChildRelationship{TParent,TChild}" />
        /// </summary>
        [Test]
        [TestCase("2021-08-01T12:00:00Z", false)] // not there yet
        [TestCase("2021-08-01T20:00:00Z", true)] // the concert just began
        [TestCase("2021-08-01T21:00:30Z", true)] // enjoying the show
        [TestCase("2021-08-01T22:00:00Z", false)] // leaving the concert hall
        [TestCase("2021-08-01T23:00:00Z", false)] // after
        public void TestMusicianListenerRelationship(string dateTimeString, bool expectedAtConcert)
        {
            var me = new Listener();
            var myFavouriteArtist = new Musician();
            // if I stayed at the gasoline pump for five minutes, the allocation looks like this:
            var relationship = new ListeningExperience
            {
                Start = new DateTimeOffset(2021, 8, 1, 20, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2021, 8, 1, 22, 0, 0, TimeSpan.Zero),
                Parent = myFavouriteArtist,
                Child = me,
                ListeningType = ListeningType.Live
            };
            Assert.AreEqual(TimeSpan.FromMinutes(120), relationship.Duration);
            var iAmAtMyFavouriteArtistsConcert = relationship.Overlaps(DateTimeOffset.Parse(dateTimeString)) && relationship.ListeningType == ListeningType.Live;
            Assert.AreEqual(expectedAtConcert, iAmAtMyFavouriteArtistsConcert);
        }
    }
}