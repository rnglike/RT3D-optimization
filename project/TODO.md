- [x] Assign a controlled material for each geometry
- [x] Verify research scripts
- [x] Verify optimization metrics
- [x] Create camera position 360 degrees sampling
- [x] Export environment information (model, x, y, polycount, shader amount, material amount, texture amount)
- [ ] Execute camera sampling in build
- [ ] Plot model x,y in 2D topdown or 3D using Python
- [x] Plot sampling in graph form using Python
- [ ] Plot sampling in 2D topdown with direction and color coded ms using Python
- [ ] Compare project with occlusion on/off (no more optimization)
- [ ] Discuss about the occlusion frustum ~situation~ and how 3D wip environments can benefit of information based occlusion culling

# Pre existing knowledge
- Current settings: removal of realtime lighting, animations, more modest rendering settings, controlled materials and static batching.
- The project still have unoptimal usage of LOD system.
- The project has physics calculations but are very low.
- The project has the presence of custom scripts.

    Last Averages:
    gpu calls: 200  / 900   ; (+77%)
    polycount: 700k / 1M    ; (+30%)
    frame metrics:
        cpu time: 10ms / 6ms ; (+40%)
        gpu time: 10ms / 5ms ; (+50%)

Summary: The project has mild to good optimization regarding rendering and no known compromises regarding scripting.
Considerations: Possibly a reduction in model amount will reduce even more both metrics (cpu and gpu).
Challenge: Occlusion techniques are made to reduce models active when outside the view of the observer in exchange of some cpu time.
Even though frustum yeilds gains in any situation, closed and sparse environments are more suitable for the culling technique.
However, the intentional creation of natural occluders can be crucial in this scenarios where the environment is still in development phase.
In later stages, it will be needed close inspection to detect opportunities for assigning good natural occluders.
In that sense, the following experiment will try to show a solution for detecting existing natural occluders as well as thye best positioning.
First, it's going to show how the frustum technique can reduce model amount but in exchange produces inconsistancy in the framerate.
Then, it's going to show the relation between the polycount and the frame time in a mildly gpu optimized project.
Finally, it will elaborate on how these metrics alone can possibly yeild good detection.
On a second note, will further speculate how this method can be outstanded by a machine learning approach if there were datasets.

# What I expect from this experiment
1. sampling position and rotation won't affect ms in builds without FC
2. color coded ms will match high polycount in builds with FC
3. it will be possible to visualize the high polycount in clusters
4. maybe sampling occlusion could yield metrics for choosing existing geometry as occludees candidates? (!)
4. maybe cluster separation could yield a good estimation of new occludees? (!)