﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Simulation;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public class MultiLevelGrowingFoldingSimulatorStudy
    {
        [Test]
        [TestCase(
            "2da0", 
            "GSSGSSGYGSEKKGYLLKKSDGIRKVWQRRKCSVKNGILTISHATSNRQPAKLNLLTCQVKPNAEDKKSFDLISHNRTYHFQAEDEQDYVAWISVLTNSKEEALTMAFSGPSSG",
            @"G:\Projects\HumanGenome\generatedProteins\MultiLevelGrowingSimulation\pdb2da0.ent"
        )]
        public void RunSimulationForSequence(string pdbCode, string sequenceString, string outputFilePath)
        {
            var distributionDirectory = @"G:\Projects\HumanGenome\ramachadranDistributions";
            var sut = new MultiLevelGrowingFoldingSimulator(distributionDirectory);
            var settings = new MultiLevelGrowingFoldingSimulatorSettings
            {
            };
            var aminoAcidSequence = sequenceString
                .Select(c => c.ToAminoAcidName())
                .ToList();

            var peptide = sut.Simulate(aminoAcidSequence, settings);
            var approximatePeptideCompleter = new ApproximatePeptideCompleter(peptide);
            var completedPeptide = approximatePeptideCompleter.GetBackbone();
            File.WriteAllText(outputFilePath, PdbSerializer.Serialize(pdbCode, completedPeptide));
        }

        [Test]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs\2dc3.pdb")]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb1yzg.ent")]
        public void MeasureAverageAminoAcidDistance(string pdbFilePath)
        {
            var pdb = PdbReader.ReadFile(pdbFilePath);
            var firstChain = pdb.Models.First().Chains.First();
            var lastAminoAcid = firstChain.AminoAcids.First();
            var aminoAcidDistances = new List<UnitValue>();
            foreach (var aminoAcid in firstChain.AminoAcids.Skip(1))
            {
                var p1 = lastAminoAcid.GetAtomFromName("CA").Position;
                var p2 = aminoAcid.GetAtomFromName("CA").Position;
                var distance = p1.DistanceTo(p2);
                aminoAcidDistances.Add(distance);
            }

            var distancesInNanoMeter = aminoAcidDistances.Select(x => x.In(SIPrefix.Nano, Unit.Meter)).ToList();
            Console.WriteLine($"Average: {distancesInNanoMeter.Average():F3} nm");
            Console.WriteLine($"Median: {distancesInNanoMeter.Median():F3} nm");
            Console.WriteLine($"Minimum: {distancesInNanoMeter.Min():F3} nm");
            Console.WriteLine($"Maximum: {distancesInNanoMeter.Max():F3} nm");
            distancesInNanoMeter.ForEach(Console.WriteLine);
        }

        [Test]
        public void StackInit()
        {
            // Does stack initialized from list return first or last item?
            var stack = new Stack<int>(new []{ 1, 2});
            Assert.That(stack.Peek(), Is.EqualTo(2)); // Answer: Last
        }
    }
}
