using System;
using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IBatallaRepository : IRepository<Batalla>
    {
        IEnumerable<Batalla> ObtenerBatallasActivas();
        bool FinalizarBatalla(int batallaId, int? ganadorId, int numeroTurno, int vidaC1, int vidaC2);
        void ProcesarAccionCombate(int batallaId, int atacanteId, int defensorId, int danoRealizado, int nuevaVidaDefensor, string? habilidadUsada, int numeroTurno);
        void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, string? habilidadUsada, int danoCausado, int nuevaVidaDefensor);
        IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta);
    }
}