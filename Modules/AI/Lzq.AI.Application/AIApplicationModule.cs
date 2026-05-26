using Lzq.AI.Application.Contracts;
using Lzq.Core.Modules;
using Lzq.Extensions.AI;

namespace Lzq.AI.Application;

[DependsOn(typeof(AIApplicationContractsModule), typeof(AIModule))]
public class AIApplicationModule : BaseModule
{
}
