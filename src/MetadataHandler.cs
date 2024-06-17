﻿using System;
using Landis.Library.Metadata;
using Landis.Core;
using System.IO;


namespace Landis.Extension.Output.Biomass
{
    public static class MetadataHandler
    {
        public static ExtensionMetadata Extension { get; set; }
        
        public static void InitializeMetadata(int Timestep, string summaryLogName, bool makeTable)
        {

            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore)
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            if (makeTable)
            {
                CreateDirectory(summaryLogName);
                PlugIn.summaryLog = new MetadataTable<SummaryLog>(summaryLogName);

                PlugIn.ModelCore.UI.WriteLine("   Generating summary table...");
                OutputMetadata tblOut_summary = new OutputMetadata()
                {
                    Type = OutputType.Table,
                    Name = "SummaryLog",
                    FilePath = PlugIn.summaryLog.FilePath,
                    Visualize = true,
                };
                tblOut_summary.RetriveFields(typeof(SummaryLog));
                Extension.OutputMetadatas.Add(tblOut_summary);
            }

            //2 kinds of maps: species and pool maps, maybe multiples of each?
            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------


            foreach(ISpecies species in PlugIn.speciesToMap)
            {
                OutputMetadata mapOut_Species = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = species.Name,
                    FilePath = SpeciesMapNames.ReplaceTemplateVars(PlugIn.speciesTemplateToMap,
                                                       species.Name,
                                                       PlugIn.ModelCore.CurrentTime),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_Species);
            }

            OutputMetadata mapOut_TotalBiomass = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "TotalBiomass",
                FilePath = SpeciesMapNames.ReplaceTemplateVars(PlugIn.speciesTemplateToMap,
                                       "TotalBiomass",
                                       PlugIn.ModelCore.CurrentTime),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_TotalBiomass);

            if(PlugIn.poolsToMap == "both" || PlugIn.poolsToMap == "woody")
            {
                OutputMetadata mapOut_WoodyDebris = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "WoodyDebrisMap",
                    FilePath = PoolMapNames.ReplaceTemplateVars(PlugIn.poolsTemplateToMap,
                                                           "woody",
                                                           PlugIn.ModelCore.CurrentTime),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_WoodyDebris);
            }
            if(PlugIn.poolsToMap == "non-woody" || PlugIn.poolsToMap == "both")
            {
                OutputMetadata mapOut_NonWoodyDebris = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "NonWoodyDebrisMap",
                    FilePath = PoolMapNames.ReplaceTemplateVars(PlugIn.poolsTemplateToMap,
                                           "non-woody",
                                           PlugIn.ModelCore.CurrentTime),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_NonWoodyDebris);
            }

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
        public static void CreateDirectory(string path)
        {
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Landis.Utilities.Directory.EnsureExists(dir);
            }

            return;
        }
    }
}