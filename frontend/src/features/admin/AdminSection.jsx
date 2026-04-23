import { CrudPanel } from '../../components'

export function AdminSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  return (
    <>
      <CrudPanel entityKey="users" rows={data.users} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-users')} />
      <CrudPanel entityKey="roles" rows={data.roles} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-roles')} />
    </>
  )
}
