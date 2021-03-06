//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Developer Projects/Unity/Balls/Assets/Scripts/TS/Grammars/TS.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace TS {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="TSParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public interface ITSVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStart([NotNull] TSParser.StartContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDecl([NotNull] TSParser.DeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.package_decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPackage_decl([NotNull] TSParser.Package_declContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.fn_decl_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFn_decl_stmt([NotNull] TSParser.Fn_decl_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.var_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVar_list([NotNull] TSParser.Var_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.statement_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement_list([NotNull] TSParser.Statement_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStmt([NotNull] TSParser.StmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.datablock_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDatablock_stmt([NotNull] TSParser.Datablock_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.object_decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObject_decl([NotNull] TSParser.Object_declContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.parent_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParent_block([NotNull] TSParser.Parent_blockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.object_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObject_name([NotNull] TSParser.Object_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.object_args"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObject_args([NotNull] TSParser.Object_argsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.object_declare_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObject_declare_block([NotNull] TSParser.Object_declare_blockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.object_decl_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObject_decl_list([NotNull] TSParser.Object_decl_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.stmt_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStmt_block([NotNull] TSParser.Stmt_blockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.switch_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitch_stmt([NotNull] TSParser.Switch_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.case_block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCase_block([NotNull] TSParser.Case_blockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.case_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCase_expr([NotNull] TSParser.Case_exprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.if_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIf_stmt([NotNull] TSParser.If_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.while_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhile_stmt([NotNull] TSParser.While_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.for_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFor_stmt([NotNull] TSParser.For_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.expression_stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression_stmt([NotNull] TSParser.Expression_stmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.slot_assign_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSlot_assign_list([NotNull] TSParser.Slot_assign_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.slot_assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSlot_assign([NotNull] TSParser.Slot_assignContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.aidx_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAidx_expr([NotNull] TSParser.Aidx_exprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.expr_list_decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_list_decl([NotNull] TSParser.Expr_list_declContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.expr_list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_list([NotNull] TSParser.Expr_listContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr([NotNull] TSParser.ExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.class_name_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClass_name_expr([NotNull] TSParser.Class_name_exprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.assign_op_struct"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssign_op_struct([NotNull] TSParser.Assign_op_structContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="TSParser.stmt_expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStmt_expr([NotNull] TSParser.Stmt_exprContext context);
}
} // namespace TS
