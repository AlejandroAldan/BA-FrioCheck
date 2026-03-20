using BA.Backend.Application.Users.Commands;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener usuario
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Usuario no existe");

        // 2. Validar tenant match
        if (user.TenantId != request.TenantId)
            throw new InvalidOperationException("No tienes permiso para eliminar este usuario");

        // 3. Soft delete (marcar como inactivo)
        user.IsActive = false;

        // 4. Guardar
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Unit.Value;
    }
}
