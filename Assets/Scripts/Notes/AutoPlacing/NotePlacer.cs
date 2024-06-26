using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNotePlacer", menuName = "Trombone Hero/Music/Note Placer")]
public class NotePlacer : ScriptableObject
{   
    private struct NotePlacementRanking
    { 
        public Vector2[] coordinates;
        public int bestPlacementIndex;
    }

    public enum DistanceType { Circle, Square, Diamond, Horizontal, Vertical }
    public enum DelayType { AlwaysInfinity, AlwaysZero, StartToStart, EndToStart, MiddleToMiddle }

    [Header("Grid")]
    public float[] lineTones;
    public int columns = 8;
    public float columnPerTone = 1f;
    [Header("Note placement")]
    public int idealNoteDistance = 2;
    public DistanceType distanceMode;
    public bool preferSameY = true;
    public float timeRadius = .5f;
    public DelayType delayMode;
    public bool groupRepeatedNotes = true;
    public int preventMoveOnSameTone;
    public int placementPasses = 100;

    public Vector2[] ToneToCoordinates(float tone)
    {
        // Find all possible coordinate (slide, pressure) for tone
        List<Vector2> coordinates = new List<Vector2>();
        for (int y = 0, ymax = lineTones.Length; y < ymax; y++)
        {
            float vTone = lineTones[y];
            if (vTone >= tone)
            {
                Vector2 coord;
                coord.y = y;
                // Deduce x from y
                coord.x = (vTone - tone) * columnPerTone;
                if (coord.x >= 0 && coord.x < columns)
                    coordinates.Add(coord);
            }
        }
        return coordinates.ToArray();
    }
    

    private float NoteDistance(Vector2 coordinates1, Vector2 coordinates2)
    {
        // Various ways of evaluating distances on grid
        switch (distanceMode)
        {
            case DistanceType.Circle: return Vector2.Distance(coordinates1, coordinates2);
            case DistanceType.Diamond: return Mathf.Abs(coordinates1.x - coordinates2.x) + Mathf.Abs(coordinates1.y - coordinates2.y);
            case DistanceType.Square: return Mathf.Max(Mathf.Abs(coordinates1.x - coordinates2.x), Mathf.Abs(coordinates1.y - coordinates2.y));
            case DistanceType.Horizontal: return Mathf.Abs(coordinates1.x - coordinates2.x);
            case DistanceType.Vertical: return Mathf.Abs(coordinates1.y - coordinates2.y);
            default: return 0f;
        }
    }

    private float NoteDelay(NoteInfo noteA, NoteInfo noteB)
    {
        // Various ways of evaluating time between two notes
        switch (delayMode)
        {
            case DelayType.AlwaysInfinity: return float.PositiveInfinity;
            case DelayType.AlwaysZero: return 0f;
            case DelayType.StartToStart:
                return Mathf.Abs(noteA.startTime - noteB.startTime);
            case DelayType.EndToStart:
                if (noteA.EndTime <= noteB.startTime) return noteB.startTime - noteA.EndTime;
                if (noteB.EndTime <= noteA.startTime) return noteA.startTime - noteB.EndTime;
                return 0f;
            case DelayType.MiddleToMiddle:
                return Mathf.Abs((noteA.startTime + noteA.duration / 2f) - (noteB.startTime + noteB.duration / 2f));
            default:
                return 0f;
        }
    }

    public void PlaceNotes(ref NoteInfo[] notes)
    {
        if (notes == null) return;
        // 1: Find all possible coordinates for each notes
        int noteCount = notes.Length;
        NotePlacementRanking[] notePlacements = new NotePlacementRanking[noteCount];
        for (int n = 0; n < noteCount; n++)
        {
            // By default, placements are sorted from lower to higher x coordinate
            Vector2[] possibleCoordinates = ToneToCoordinates(notes[n].tone);
            notePlacements[n] = new NotePlacementRanking() { coordinates = possibleCoordinates };
        }
        // 2: Modify coordinates fit by considering neighbour notes
        if (noteCount > 1)
        {
            int passCounter = 0;
            for (int n = 0; n < noteCount; n++)
            {
                Debug.Log("Note #" + n);
                int bestPlacementIndex = notePlacements[n].bestPlacementIndex;
                if (n > 0 && n < noteCount - 1)
                    bestPlacementIndex = GetBestPlacementIndex(notePlacements[n], notePlacements[n - 1], notePlacements[n + 1]);
                else if (n == 0)
                    bestPlacementIndex = GetBestPlacementIndex(notePlacements[n], notePlacements[n + 1]);
                else if (n == noteCount - 1)
                    bestPlacementIndex = GetBestPlacementIndex(notePlacements[n], notePlacements[n - 1]);
                if (bestPlacementIndex != notePlacements[n].bestPlacementIndex)
                {
                    Debug.Log("Pass " + passCounter + "/" + placementPasses + " -> change at note #" + n + " from placement " + notePlacements[n].bestPlacementIndex + " to " + bestPlacementIndex);
                    notePlacements[n].bestPlacementIndex = bestPlacementIndex;
                    passCounter++;
                    n = 0;
                }
                if (passCounter >= placementPasses) break;
            }
        }
        // 3: Set best fitting coordinates
        Debug.LogWarning("NOTE PLACING NOT IMPLEMENTED");
        //for (int n = 0; n < noteCount; n++)
        //{
        //    if (notePlacements[n].coordinates == null || notePlacements[n].coordinates.Length == 0) Debug.LogWarning("Couldnt place note #" + n);
        //    else notes[n].gridCoordinates = notePlacements[n].coordinates[notePlacements[n].bestPlacementIndex];
        //}
    }

    private int GetBestPlacementIndex(NotePlacementRanking placement, params NotePlacementRanking[] relatives)
    {
        int placementCount = placement.coordinates != null ? placement.coordinates.Length : 0;
        if (placementCount == 0)
        {
            Debug.LogWarning("No placement found.");
            return -1;
        }
        if (placementCount == 1)
        {
            Debug.Log("Only one placement found.");
            return 0;
        }
        int bestIndex = -1;
        float bestDistanceSum = float.PositiveInfinity;
        for (int p = 0; p < placementCount; p++)
        {
            Vector2 coordinates = placement.coordinates[p];
            float distanceSum = 0;
            Debug.Log("- placement " + p + " : " + coordinates.ToString());
            foreach (NotePlacementRanking relative in relatives)
            {
                if (relative.coordinates != null)
                {
                    for (int c = 0, cend = relative.coordinates.Length; c < cend; c++)
                    {
                        Vector2 otherCoordinates = relative.coordinates[c];
                        if (c == relative.bestPlacementIndex)
                        {
                            Debug.Log(" + " + NoteDistance(coordinates, otherCoordinates));
                            distanceSum += NoteDistance(coordinates, otherCoordinates);
                        }
                        else
                        {
                            Debug.Log(" + " + NoteDistance(coordinates, otherCoordinates) + " / 2");
                            distanceSum += NoteDistance(coordinates, otherCoordinates) / 4f;
                        }
                    }
                }
            }
            Debug.Log("=> distanceSum = " + distanceSum);
            if (distanceSum < bestDistanceSum)
            {
                bestDistanceSum = distanceSum;
                bestIndex = p;
            }
        }
        return bestIndex;
    }


    // Methode avec des scores/poids
    //public void PlaceNotes(ref NoteInfo[] notes)
    //{
    //    if (notes == null) return;
    //    // 1: Find all possible coordinates for each notes
    //    int noteCount = notes.Length;
    //    NotePlacementRating[] notePlacements = new NotePlacementRating[noteCount];
    //    for (int n = 0; n < noteCount; n++)
    //    {
    //        Vector2[] possibleCoordinates = ToneToCoordinates(notes[n].tone);
    //        notePlacements[n] = new NotePlacementRating(possibleCoordinates);
    //    }
    //    // 2: Modify coordinates fit by considering neighbour notes
    //    Debug.Log("Note placement: ");
    //    for (int p = 0; p < placementPasses; p++)
    //    {
    //        NotePlacementRating[] newNotePlacements = new NotePlacementRating[noteCount];
    //        int changeCounter = 0;
    //        for (int n = 0; n < noteCount; n++)
    //        {
    //            NotePlacementRating newNotePlacement = new NotePlacementRating(notePlacements[n]);
    //            if (n > 0) newNotePlacement = RateNotePlacementRelative(newNotePlacement, notePlacements[n - 1], true);
    //            if (n < noteCount - 1) newNotePlacement = RateNotePlacementRelative(newNotePlacement, notePlacements[n + 1], false);
    //            newNotePlacements[n] = newNotePlacement;
    //            if (newNotePlacements[n].BestCoordinatesIndex != notePlacements[n].BestCoordinatesIndex) changeCounter++;
    //        }
    //        notePlacements = newNotePlacements;
    //        Debug.Log("-> pass " + p + "/" + placementPasses);
    //        Debug.Log("-> changes: " + changeCounter);
    //        // Stop if no changes
    //        //if (changeCounter == 0) break;
    //    }
    //    // 3: Set best fitting coordinates
    //    int movedNotesCounter = 0;
    //    for (int n = 0; n < noteCount; n++)
    //    {
    //        int bestPlacementIndex = notePlacements[n].BestCoordinatesIndex;
    //        if (bestPlacementIndex != 0) movedNotesCounter++;
    //        if (bestPlacementIndex < 0 || bestPlacementIndex >= notePlacements[n].coordinates.Length) Debug.LogWarning("Couldnt place note #" + n);
    //        else notes[n].gridCoordinates = notePlacements[n].coordinates[bestPlacementIndex];
    //    }
    //    Debug.Log("-> " + movedNotesCounter + " moved notes");
    //}

    //private NotePlacementRating RateNotePlacementRelative(NotePlacementRating notePlacement, NotePlacementRating otherNotePlacement, bool direction)
    //{
    //    NotePlacementRating newNotePlacement = notePlacement;
    //    int coordinatesCount = notePlacement.coordinates.Length;
    //    int otherCoordinatesCount = otherNotePlacement.coordinates.Length;
    //    for (int i = 0; i < coordinatesCount; i++)
    //        for (int j = 0; j < otherCoordinatesCount; j++)
    //        {
    //            // Rate note placement over distance with other note
    //            float distance = NoteDistance(notePlacement.coordinates[i], otherNotePlacement.coordinates[j]);
    //            float distanceFit = (columns - distance) / columns;
    //            // Final placement rate
    //            if (direction == true) newNotePlacement.fit_direct[i] += distanceFit * otherNotePlacement.fit_direct[j];
    //            else newNotePlacement.fit_reverse[i] += distanceFit * otherNotePlacement.fit_reverse[j];
    //        }
    //    return newNotePlacement;
    //}

    // Méthode avec plein de critères
    /*
    public void PlaceNotes(ref NoteInfo[] notes)
    {
        if (notes == null) return;
        int noteCount = notes.Length;
        // Place notes with default position (slide up, lowest pressure)
        for (int n = 0; n < noteCount; n++)
        {
            Vector2[] possibleCoordinates = ToneToCoordinates(notes[n].tone);
            if (possibleCoordinates.Length > 0) notes[n].gridCoordinates = possibleCoordinates[0];
        }
        // Exception for one-note song
        if (noteCount <= 1) return;
        // Optimiziation (back and forth)        
        for (int passes = 0; passes <= placementPasses; passes++)
        {
            for (int n = 1; n < noteCount; n++) PlaceNotes(ref notes, n, false);
            for (int n = noteCount - 2; n >= 0; n--) PlaceNotes(ref notes, n, true);
        }
    }

    private void PlaceNotes(ref NoteInfo[] notes, int noteIndex, bool reverseCheck)
    {
        int lookDirection = reverseCheck ? 1 : -1;
        // Simple exception: same tone, same position
        int otherNoteIndex = noteIndex;
        float otherNoteTone = notes[noteIndex].tone;
        for (int i = 1, iend = preventMoveOnSameTone; i < iend; i++)
        {
            otherNoteIndex = noteIndex + lookDirection * i;
            if (otherNoteIndex < 0 || otherNoteIndex >= notes.Length - 1) break;
            if (notes[otherNoteIndex].tone == otherNoteTone)
            {
                iend++;
                continue;
            }
            otherNoteTone = notes[otherNoteIndex].tone;
            if (notes[noteIndex].tone == otherNoteTone)
            {
                notes[noteIndex].gridCoordinates = notes[otherNoteIndex].gridCoordinates;
                return;
            }
        }
        // Another exception: don't try to place notes relative to each other if they are too distant in time
        if (NoteDelay(notes[noteIndex], notes[noteIndex + lookDirection]) > timeRadius)
            return;
        // Find all possible coordinates for note
        Vector2 nextNoteCoordinates = notes[noteIndex + lookDirection].gridCoordinates;
        Vector2[] possibleCoordinates = ToneToCoordinates(notes[noteIndex].tone);
        int coordinatesCount = possibleCoordinates != null ? possibleCoordinates.Length : 0;
        // If one possibility or less, stop here
        if (coordinatesCount == 0)
        {
            notes[noteIndex].gridCoordinates = new Vector2(-1f, -1f);
            return;
        }
        if (coordinatesCount == 1)
        {
            notes[noteIndex].gridCoordinates = possibleCoordinates[0];
            return;
        }
        // If two or more, keep coordinates that are close to previous or next note
        List<Vector2> idealCoordinates = new List<Vector2>(coordinatesCount);
        foreach (Vector2 c in possibleCoordinates)
        {
            if (NoteDistance(c, nextNoteCoordinates) <= idealNoteDistance)
                idealCoordinates.Add(c);
        }
        // If one possibility or less, stop here
        coordinatesCount = idealCoordinates.Count;
        if (coordinatesCount == 0)
        {
            notes[noteIndex].gridCoordinates = possibleCoordinates[0];
            return;
        }
        if (coordinatesCount == 1)
        {
            notes[noteIndex].gridCoordinates = idealCoordinates[0];
            return;
        }
        // If still more than two choices, see if one is on same height as previous or next note
        if (preferSameY)
        {
            foreach (Vector2 c in possibleCoordinates)
            {
                if (c.y == nextNoteCoordinates.y)
                {
                    notes[noteIndex].gridCoordinates = c;
                    return;
                }
            }
        }
        // By default, chose first coordinates
        notes[noteIndex].gridCoordinates = possibleCoordinates[0];
    }
    */
}