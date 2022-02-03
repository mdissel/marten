﻿using System.Linq;
using Marten;
using Marten.Linq;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Shouldly;
using Xunit;

namespace DocumentDbTests.Reading.Linq
{
    public class previewing_the_command_from_a_queryable_Tests : OneOffConfigurationsContext
    {

        [Fact]
        public void preview_basic_select_command()
        {
            var cmd = theSession.Query<Target>().ToCommand(FetchType.FetchMany);

            cmd.CommandText.ShouldBe($"select d.id, d.data from {SchemaName}.mt_doc_target as d");
            cmd.Parameters.Any().ShouldBeFalse();
        }

        [Fact]
        public void preview_command_with_where_and_parameters()
        {
            var cmd = theSession.Query<Target>().Where(x => x.Number == 3 && x.Double > 2).ToCommand(FetchType.FetchMany);

            cmd.CommandText.ShouldBe($"select d.id, d.data from {SchemaName}.mt_doc_target as d where (CAST(d.data ->> 'Number' as integer) = :p0 and CAST(d.data ->> 'Double' as double precision) > :p1)");

            cmd.Parameters.Count.ShouldBe(2);
            cmd.Parameters["p0"].Value.ShouldBe(3);
            cmd.Parameters["p1"].Value.ShouldBe(2);
        }

        [Fact]
        public void preview_basic_count_command()
        {
            var cmd = theSession.Query<Target>().ToCommand(FetchType.Count);

            cmd.CommandText.ShouldBe($"select count(*) as number from {SchemaName}.mt_doc_target as d");
        }

        [Fact]
        public void preview_basic_any_command()
        {
            var cmd = theSession.Query<Target>().ToCommand(FetchType.Any);

            cmd.CommandText.ShouldBe($"select TRUE as result from {SchemaName}.mt_doc_target as d LIMIT :p0");
        }

        [Fact]
        public void preview_select_on_query()
        {
            var cmd = theSession.Query<Target>().OrderBy(x => x.Double).ToCommand(FetchType.FetchOne);

            cmd.CommandText.Trim().ShouldBe($"select d.id, d.data from {SchemaName}.mt_doc_target as d order by CAST(d.data ->> 'Double' as double precision) LIMIT :p0");
        }


    }

    public class previewing_the_command_from_a_queryable_in_a_different_schema_Tests : previewing_the_command_from_a_queryable_Tests
    {
        public previewing_the_command_from_a_queryable_in_a_different_schema_Tests()
        {
            StoreOptions(_ => _.DatabaseSchemaName = SchemaName);
        }
    }
}
