# Mapping conventions

Mappers perform **strict DTO ↔ entity translation** only.

## Allowed methods

| Method | Direction |
|--------|-----------|
| `FromDto` | DTO → new entity |
| `ToDto` | Entity → DTO |
| `UpdateFromDto` | DTO → existing entity |

## Not allowed in mappers

- Calculations → `Helpers/*Calculator`, `Helpers/*Helper`
- State transitions (approve, reject, status) → `Helpers/*Workflow`
- Soft delete / restore → `Helpers/*Lifecycle`
- Business rules → services and policies

## Examples

```csharp
var employee = EmployeeMapper.FromDto(createDto);
EmployeeMapper.UpdateFromDto(employee, updateDto);
var item = UserAccountMapper.ToDto(row, role);
PayrollCalculator.ApplyTotals(payroll, bonus, deduction);
LeaveWorkflow.Approve(request, approverId);
EmployeeLifecycle.MarkDeleted(employee);
```

See `Helpers/` for non-mapping entity operations.
