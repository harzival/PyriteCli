﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using clipr;
using clipr.Core;
using CuberLib;

namespace Cuber
{
    class Program
    {
		// Note: This will probably only work well with OBJ files generated by Pix4D
		// as I have only supported the subset of data types it outputs.
		static void Main(string[] args)
        {
			Options opt;

			// Setup a timer for all operations
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				opt = CliParser.Parse<Options>(args);

				foreach (string path in opt.Input)
				{
					// Check if we are processing an image or a mesh
					if (Path.GetExtension(path).ToUpper().EndsWith("JPG"))
					{
						ImageTile tiler = new ImageTile(path, opt.xSize, opt.ySize);
						tiler.GenerateTiles(opt.OutputPath);
					}
					else if (Path.GetExtension(path).ToUpper().EndsWith("OBJ"))
					{
						// Generate subfolders named after input file
						// if multiple input files are provided
						string outputPath;
						if (opt.Input.Count == 1)
						{
							outputPath = opt.OutputPath;
						}
						else
						{
							outputPath = Path.Combine(opt.OutputPath, Path.GetFileNameWithoutExtension(path));
						}

						var options = new SlicingOptions
						{
							OverrideMtl = opt.MtlOverride,
							GenerateEbo = opt.Ebo,
							AttemptResume = opt.Resume,
							GenerateObj = true
						};

						CubeManager manager = new CubeManager(path, opt.xSize, opt.ySize, opt.zSize);						
						manager.GenerateCubes(outputPath, options);
					}
					else
					{
						Console.WriteLine("Cuber only accepts .jpg and .obj files for input.");
					}
				}
			}
			catch (ParserExit)
			{
				return;
			}
			catch (ParseException)
			{
				Console.WriteLine("usage: Cuber --help");
			}

			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed.ToString());
		}		
		
    }

	[ApplicationInfo(Description = "Cuber Options")]
	public class Options
	{
		[NamedArgument('x', "xsize", Action = ParseAction.Store,
			Description = "The number of times to subdivide in the X dimension.  Default 10.")]
		public int xSize { get; set; }

		[NamedArgument('y', "ysize", Action = ParseAction.Store,
			Description = "The number of times to subdivide in the Y dimension.  Default 10.")]
		public int ySize { get; set; }

		[NamedArgument('z', "zsize", Action = ParseAction.Store,
			Description = "The number of times to subdivide in the Z dimension.  Default 10.")]
		public int zSize { get; set; }

		[NamedArgument('m', "mtl", Action = ParseAction.Store,
			Description = "Override the MTL field in output obj files. e.g. -z model.mtl")]
		public string MtlOverride { get; set; }

		[NamedArgument('e', "ebo", Action = ParseAction.StoreTrue,
			Description = "Generate EBO files designed for use with CubeServer in addition to OBJ files")]
		public bool Ebo { get; set; }

		[NamedArgument('d', "debug", Action = ParseAction.StoreTrue,
			Description = "flag for testing prototype features")]
		public bool DebugTest { get; set; }

		[NamedArgument('r', "resume", Action = ParseAction.StoreTrue,
			Description = "Existing files are not overwritten, but instead skipped")]
		public bool Resume { get; set; }

		[PositionalArgument(0, MetaVar = "OUT",
			Description = "Output folder")]
		public string OutputPath { get; set; }

		[PositionalArgument(1, MetaVar = "IN",
			NumArgs = 1,
			Constraint = NumArgsConstraint.AtLeast,
			Description = "A list of .obj files to process")]
		public List<string> Input { get; set; }

		public Options()
		{
			xSize = 10;
			ySize = 10;
			zSize = 10;
		}
	}

}
